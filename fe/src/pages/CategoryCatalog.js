// src/pages/CategoryCatalog.js
import React, { useEffect, useMemo, useRef, useState } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Toolbar } from "primereact/toolbar";
import { InputText } from "primereact/inputtext";
import { InputNumber } from "primereact/inputnumber";
import { Tag } from "primereact/tag";
import { Button } from "primereact/button";
import { Dialog } from "primereact/dialog";
import { Divider } from "primereact/divider";
import { createApi, getAxiosMessage } from "../lib/api";

export default function CategoryCatalogPage({ baseUrl, onToast }) {
  const api = useMemo(() => createApi(baseUrl), [baseUrl]);

  const [q, setQ] = useState("");
  const [filterCatId] = useState(null);
  const [minPrice, setMinPrice] = useState(null);
  const [maxPrice, setMaxPrice] = useState(null);

  const [page, setPage] = useState(1);
  const [size, setSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(false);

  const [categories, setCategories] = useState([]);
  const catMap = useMemo(() => {
    const m = new Map();
    categories.forEach((c) => m.set(c.id, c));
    return m;
  }, [categories]);

  const catName = (id) => catMap.get(id)?.name || id;

  const dt = useRef(null);

  // State cho dialog chi tiết
  const [detailOpen, setDetailOpen] = useState(false);
  const [detail, setDetail] = useState(null);

  const loadCategories = async () => {
    try {
      const { data } = await api.get("/api/categories", { params: { page: 1, size: 999 } });
      setCategories(data?.items || []);
    } catch (err) {
      onToast("warn", "Không tải được danh mục", getAxiosMessage(err));
    }
  };

  const loadingData = async () => {
    setLoading(true);
    try {
      const { data } = await api.get("/api/catalogs", {
        params: { page, size, q, categoryId: filterCatId, minPrice, maxPrice, isActive: true },
      });
      setRows(data?.items || []);
      setTotal(data?.total || 0);
    } catch (err) {
      onToast("error", "Tải dữ liệu thất bại", getAxiosMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadCategories(); }, []);
  useEffect(() => { loadingData(); /* eslint-disable-next-line */ }, [page, size]);

  const onSearch = () => { setPage(1); loadingData(); };

  const imgBody = (r) =>
    r.imageUrl ? (
      <img
        src={r.imageUrl}
        alt={r.name}
        preview
        style={{ width: 150, height: 150, objectFit: "cover", borderRadius: 6, display: "block" }}
      />
    ) : (
      <span className="pi pi-image text-400" />
    );

  // Mở dialog khi click hàng
  const openDetail = (row) => {
    setDetail(row);
    setDetailOpen(true);
  };

  const Header = (
    <Toolbar
      left={() => (
        <div>
          <span className="p-input-icon-left" style={{ marginRight: 8 }}>
            <InputText
              value={q}
              onChange={(e) => setQ(e.target.value)}
              placeholder="Tìm kiếm"
              onKeyDown={(e) => e.key === "Enter" && onSearch()}
            />
          </span>
          {/* Nếu cần lọc giá thì mở comment đoạn dưới
          <span style={{ marginRight: 8 }}>
            Giá từ{" "}
            <InputNumber
              value={minPrice}
              onValueChange={(e) => setMinPrice(e.value)}
              mode="currency"
              currency="USD"
              locale="en-US"
            />{" "}
            đến{" "}
            <InputNumber
              value={maxPrice}
              onValueChange={(e) => setMaxPrice(e.value)}
              mode="currency"
              currency="USD"
              locale="en-US"
            />
          </span>
          */}
          <Button icon="pi pi-filter" label="Lọc" onClick={onSearch} style={{ marginRight: 8 }} />
          <Button icon="pi pi-refresh" className="p-button-text" onClick={loadingData} />
        </div>
      )}
      right={() => (
        <div>
          <Button icon="pi pi-download" label="Export CSV" onClick={() => dt.current?.exportCSV()} />
        </div>
      )}
    />
  );

  return (
    <div>
      {Header}

      <DataTable
        ref={dt}
        value={rows}
        loading={loading}
        paginator
        lazy
        rows={size}
        totalRecords={total}
        first={(page - 1) * size}
        onPage={(e) => {
          setPage(Math.floor(e.first / e.rows) + 1);
          setSize(e.rows);
        }}
        rowsPerPageOptions={[5, 10, 20, 50]}
        emptyMessage="Không có dữ liệu"
        onRowClick={(e) => openDetail(e.data)}
        rowClassName={() => "cursor-pointer"}
        rowHover
      >
        <Column field="id" header="#ID" style={{ width: 90 }} />
        <Column header="Ảnh" body={imgBody} style={{ width: 200 }} />
        <Column field="name" header="Catalog Name" sortable />
        <Column field="code" header="Code" sortable />
        <Column header="Category" body={(r) => catName(r.categoryId)} />
        <Column
          header="Active"
          body={(r) => (
            <Tag
              value={r.isActive ? "Active" : "Inactive"}
              severity={r.isActive ? "success" : "secondary"}
            />
          )}
          style={{ width: 140 }}
        />
      </DataTable>

      {/* Dialog chi tiết */}
      <Dialog
        header="Thông tin sản phẩm"
        visible={detailOpen}
        onHide={() => setDetailOpen(false)}
        style={{ width: "46rem", maxWidth: "95vw" }}
        modal
      >
        {detail && (
          <div className="flex flex-column gap-3">
            <div className="flex gap-3">
              <div style={{ width: 180, height: 180, borderRadius: 12, overflow: "hidden" }}>
                {detail.imageUrl ? (
                  <img
                    src={detail.imageUrl}
                    alt={detail.name}
                    style={{ width: "100%", height: "100%", objectFit: "cover" }}
                  />
                ) : (
                  <div
                    className="flex align-items-center justify-content-center surface-200"
                    style={{ width: "100%", height: "100%" }}
                  >
                    <span className="pi pi-image text-500" style={{ fontSize: 24 }} />
                  </div>
                )}
              </div>

              <div className="flex-1">
                <div className="text-xl font-semibold">{detail.name}</div>
                <div className="text-600">Code: {detail.code || "—"}</div>
                <div>Danh mục: {catName(detail.categoryId)}</div>
                {/* {"price" in detail && (
                  <div style={{ fontWeight: 600, marginTop: 6 }}>
                    {(Number(detail.price) || 0).toLocaleString(undefined, {
                      style: "currency",
                      currency: "USD",
                    })}
                  </div>
                )} */}
                <div className="mt-2">
                  <Tag
                    value={detail.isActive ? "Active" : "Inactive"}
                    severity={detail.isActive ? "success" : "secondary"}
                  />
                </div>
              </div>
            </div>

            <Divider />
            <div>
              <div className="text-700" style={{ whiteSpace: "pre-wrap" }}>
                {detail.description || "Không có mô tả."}
              </div>
            </div>
          </div>
        )}
      </Dialog>
    </div>
  );
}
