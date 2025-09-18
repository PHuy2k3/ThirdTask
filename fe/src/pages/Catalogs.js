// src/pages/Catalogs.js
import React, { useEffect, useMemo, useRef, useState } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Dialog } from "primereact/dialog";
import { InputText } from "primereact/inputtext";
import { InputNumber } from "primereact/inputnumber";
import { Dropdown } from "primereact/dropdown";
import { InputSwitch } from "primereact/inputswitch";
import { Button } from "primereact/button";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import { Tag } from "primereact/tag";
import { Panel } from "primereact/panel";
import { Image } from "primereact/image";
import { Toolbar } from "primereact/toolbar";
import { Chip } from "primereact/chip";
import { Divider } from "primereact/divider";
import { InputTextarea } from "primereact/inputtextarea";
import { FloatLabel } from "primereact/floatlabel";
import { Card } from "primereact/card";
import { createApi, getAxiosMessage } from "../lib/api";

/* ---------- Cấu hình ảnh (kích thước & placeholder) ---------- */
const PLACEHOLDER_IMG = "https://via.placeholder.com/160?text=No+Image";
const thumbStyle = { width: 160, height: 160, objectFit: "cover", display: "block", borderRadius: 12 };

/* ---------- Helpers validate URL ảnh ---------- */
const MAX_IMAGEURL = 1024;
function isHttpUrl(u) {
  if (!u) return true; // cho phép rỗng
  try {
    const x = new URL(u.trim());
    return (x.protocol === "http:" || x.protocol === "https:") && u.trim().length <= MAX_IMAGEURL;
  } catch {
    return false;
  }
}
function cleanUrl(u) {
  return u?.trim() || "";
}

export default function CatalogsPage({ baseUrl, onToast }) {
  const api = useMemo(() => createApi(baseUrl), [baseUrl]);

  const [q, setQ] = useState("");
  const [page, setPage] = useState(1);
  const [size, setSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(false);

  const [categories, setCategories] = useState([]);
  const [filterCatId, setFilterCatId] = useState(null);
  const [minPrice, setMinPrice] = useState(null);
  const [maxPrice, setMaxPrice] = useState(null);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editId, setEditId] = useState(null);
  const [form, setForm] = useState({
    name: "",
    code: "",
    categoryId: 0,
    price: 0,
    imageUrl: "",
    description: "",
    isActive: true,
  });

  const dt = useRef(null);

  const catOptions = useMemo(
    () => categories.map((c) => ({ label: `${c.name} (#${c.id})`, value: c.id })),
    [categories]
  );
  const catName = (id) => categories.find((c) => c.id === id)?.name || id;

  const hasAnyFilter = !!(q || filterCatId !== null || minPrice != null || maxPrice != null);

  const clearFilter = (key) => {
    if (key === "q") setQ("");
    if (key === "cat") setFilterCatId(null);
    if (key === "min") setMinPrice(null);
    if (key === "max") setMaxPrice(null);
    setPage(1);
    setTimeout(loadingData, 0);
  };

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
        params: { page, size, q, categoryId: filterCatId, minPrice, maxPrice },
      });
      setRows(data?.items || []);
      setTotal(data?.total || 0);
    } catch (err) {
      onToast("error", "Tải catalogs thất bại", getAxiosMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadCategories(); }, []);
  useEffect(() => { loadingData(); /* eslint-disable-next-line */ }, [page, size]);

  const onSearch = () => { setPage(1); loadingData(); };

  const openNew = () => {
    setEditId(null);
    setForm({
      name: "",
      code: "",
      categoryId: categories[0]?.id ?? 0,
      price: 0,
      imageUrl: "",
      description: "",
      isActive: true,
    });
    setDialogOpen(true);
  };

  const openEdit = (r) => {
    setEditId(r.id);
    setForm({
      name: r.name,
      code: r.code,
      categoryId: r.categoryId,
      price: r.price,
      imageUrl: r.imageUrl || "",
      description: r.description || "",
      isActive: !!r.isActive,
    });
    setDialogOpen(true);
  };

  const save = async () => {
    if (!form.name?.trim() || !form.code?.trim()) {
      return onToast("warn", "Thiếu dữ liệu", "Tên và Code bắt buộc");
    }
    if (!form.categoryId) {
      return onToast("warn", "Thiếu dữ liệu", "Chọn Category");
    }

    const img = cleanUrl(form.imageUrl);
    if (img && !isHttpUrl(img)) {
      return onToast("warn", "Ảnh không hợp lệ", `Chỉ nhận link http/https, không nhận data:, tối đa ${MAX_IMAGEURL} ký tự`);
    }
    if (img.toLowerCase().startsWith("data:")) {
      return onToast("warn", "Ảnh không hợp lệ", "Không hỗ trợ ảnh base64 (data:). Vui lòng dán link http/https.");
    }

    try {
      const payload = { ...form, imageUrl: img || null, price: Number(form.price) || 0 };
      if (editId == null) {
        await api.post("/api/catalogs", payload);
        onToast("success", "Đã tạo", form.name);
      } else {
        await api.put(`/api/catalogs/${editId}`, payload);
        onToast("success", "Đã cập nhật", form.name);
      }
      setDialogOpen(false);
      loadingData();
    } catch (err) {
      onToast("error", "Lưu thất bại", getAxiosMessage(err));
    }
  };

  const remove = (r) =>
    confirmDialog({
      message: `Xoá sản phẩm "${r.name}"?`,
      header: "Xác nhận",
      icon: "pi pi-exclamation-triangle",
      acceptClassName: "p-button-danger",
      accept: async () => {
        try {
          await api.delete(`/api/catalogs/${r.id}`);
          onToast("success", "Đã xoá", r.name);
          loadingData();
        } catch (err) {
          onToast("error", "Xoá thất bại", getAxiosMessage(err));
        }
      },
    });

  const toggleActive = async (r) => {
    try {
      const payload = {
        name: r.name,
        code: r.code,
        categoryId: r.categoryId,
        price: r.price,
        imageUrl: r.imageUrl || "",
        description: r.description || "",
        isActive: !r.isActive,
      };
      await api.put(`/api/catalogs/${r.id}`, payload);
      onToast("success", !r.isActive ? "Đã kích hoạt" : "Đã tắt", r.name);
      loadingData();
    } catch (err) {
      onToast("error", "Đổi trạng thái thất bại", getAxiosMessage(err));
    }
  };

  const HeaderToolbar = (
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
          {/* <span style={{ marginRight: 8 }}>
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
          </span> */}
          <Button icon="pi pi-filter" label="Lọc" onClick={onSearch} style={{ marginRight: 8 }} />
          <Button icon="pi pi-refresh" className="p-button-text" onClick={loadingData} />
        </div>
      )}
      right={() => (
        <div>
          <Button
            icon="pi pi-download"
            label="Export CSV"
            onClick={() => dt.current?.exportCSV()}
            style={{ marginRight: 8 }}
          />
          <Button icon="pi pi-plus" label="Thêm" onClick={openNew} />
        </div>
      )}
    />
  );

  const ActiveFilterChips = hasAnyFilter && (
    <div>
      {q && <Chip label={`Tìm: ${q}`} onRemove={() => clearFilter("q")} removable style={{ marginRight: 8 }} />}
      {filterCatId !== null && (
        <Chip
          label={`Danh mục: ${catName(filterCatId)}`}
          onRemove={() => clearFilter("cat")}
          removable
          style={{ marginRight: 8 }}
        />
      )}
      {minPrice != null && (
        <Chip label={`Min: ${minPrice}`} onRemove={() => clearFilter("min")} removable style={{ marginRight: 8 }} />
      )}
      {maxPrice != null && <Chip label={`Max: ${maxPrice}`} onRemove={() => clearFilter("max")} removable />}
      <Button
        className="p-button-text"
        onClick={() => {
          setQ("");
          setFilterCatId(null);
          setMinPrice(null);
          setMaxPrice(null);
          setPage(1);
          loadingData();
        }}
        style={{ marginLeft: 8 }}
      >
        Xoá tất cả
      </Button>
    </div>
  );

  return (
    <div>
      <ConfirmDialog />

      {HeaderToolbar}
      {ActiveFilterChips}

      <Panel header="Danh sách sản phẩm">
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
        >
          <Column field="id" header="#" />

          {/* Ảnh: cùng kích thước + preview + fallback */}
          <Column
            header="Ảnh"
            body={(r) => (
              <Image
                src={r.imageUrl || PLACEHOLDER_IMG}
                alt={r.name}
                imageStyle={thumbStyle}
                onError={(e) => (e.target.src = PLACEHOLDER_IMG)}
              />
            )}
            style={{ width: 200 }}
          />

          <Column
            field="name"
            header="Tên"
            body={(r) => (
              <div>
                <div>{r.name}</div>
                {r.description && <small title={r.description}>{r.description}</small>}
              </div>
            )}
          />
          <Column field="code" header="Code" />
          <Column header="Danh mục" body={(r) => catName(r.categoryId)} />
          {/* <Column
            field="price"
            header="Giá"
            body={(r) => r.price?.toLocaleString(undefined, { style: "currency", currency: "USD" })}
          /> */}
          <Column
            header="Trạng thái"
            body={(r) => (
              <Tag
                value={r.isActive ? "Active" : "Inactive"}
                severity={r.isActive ? "success" : "secondary"}
                onClick={() => toggleActive(r)}
              />
            )}
          />
          <Column
            header="Thao tác"
            body={(r) => (
              <span>
                <Button icon="pi pi-pencil" className="p-button-text" onClick={() => openEdit(r)} />
                <Button
                  icon="pi pi-trash"
                  className="p-button-text p-button-danger"
                  onClick={() => remove(r)}
                />
              </span>
            )}
          />
        </DataTable>
      </Panel>

      <Dialog
        visible={dialogOpen}
        onHide={() => setDialogOpen(false)}
        modal
        header={
          <div style={{ fontWeight: "600", fontSize: "1.3rem", color: "#333" }}>
            {editId == null ? "Thêm sản phẩm" : "Sửa sản phẩm"}
          </div>
        }
        style={{
          width: "60rem",
          maxWidth: "95vw",
          borderRadius: 12,
          boxShadow: "0 8px 20px rgba(0,0,0,0.15)",
        }}
        breakpoints={{ "960px": "70vw", "640px": "95vw" }}
        footer={
          <div className="flex gap-3 justify-content-end" style={{ padding: "1rem" }}>
            <Button label="Huỷ" className="p-button-text" onClick={() => setDialogOpen(false)} />
            <Button label="Lưu" icon="pi pi-save" onClick={save} />
          </div>
        }
      >
        <div className="p-fluid grid formgrid" style={{ gap: "1.5rem" }}>
          {/* Thông tin cơ bản */}
          <div className="col-12">
            <Divider align="left" style={{ fontWeight: "600", fontSize: "1.1rem", color: "#555" }}>
              <span className="flex align-items-center gap-2">
                <i className="pi pi-info-circle" />
                <span>Thông tin cơ bản</span>
              </span>
            </Divider>
          </div>
          <div className="field col-12 md:col-8">
            <FloatLabel>
              <InputText
                id="name"
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                style={{ padding: "0.7rem", marginBottom: 20 }}
              />
              <label htmlFor="name">Tên *</label>
            </FloatLabel>
          </div>
          <div className="field col-12 md:col-4">
            <FloatLabel>
              <InputText
                id="code"
                value={form.code}
                onChange={(e) => setForm({ ...form, code: e.target.value })}
                style={{ marginBottom: 20 }}
              />
              <label htmlFor="code">Code *</label>
            </FloatLabel>
          </div>
          <div className="field col-12 md:col-6">
            <FloatLabel>
              <Dropdown
                id="categoryId"
                inputId="categoryId"
                value={form.categoryId}
                options={catOptions}
                onChange={(e) => setForm({ ...form, categoryId: e.value })}
                className="w-full"
                style={{ minHeight: "2.7rem" }}
              />
              <label htmlFor="categoryId">Danh mục *</label>
            </FloatLabel>
          </div>

          {/* Giá & trạng thái */}
          <div className="col-12" style={{ marginTop: "1rem" }}>
            <Divider align="left" style={{ fontWeight: "600", fontSize: "1.1rem", color: "#555" }}>
              <span className="flex align-items-center gap-2">
                <i className="pi pi-sliders-h" />
                <span>Giá & trạng thái</span>
              </span>
            </Divider>
          </div>
          <div className="field col-12 md:col-6">
            <label className="block mb-1" style={{ fontWeight: "600", color: "#444" }}>
              Giá
            </label>
            <InputNumber
              value={form.price}
              onValueChange={(e) => setForm({ ...form, price: e.value ?? 0 })}
              mode="currency"
              currency="USD"
              locale="en-US"
              className="w-full"
              style={{ padding: "0.7rem" }}
            />
          </div>
          <div className="field col-12 md:col-6">
            <label className="block mb-2" style={{ fontWeight: "600", color: "#444" }}>
              Kích hoạt
            </label>
            <div className="flex align-items-center gap-3">
              <InputSwitch
                checked={form.isActive}
                onChange={(e) => setForm({ ...form, isActive: e.value })}
              />
              <Tag
                value={form.isActive ? "Active" : "Inactive"}
                severity={form.isActive ? "success" : "secondary"}
                style={{ fontWeight: "600" }}
              />
            </div>
          </div>

          {/* Hình ảnh & mô tả */}
          <div className="col-12" style={{ marginTop: "1rem" }}>
            <Divider align="left" style={{ fontWeight: "600", fontSize: "1.1rem", color: "#555" }}>
              <span className="flex align-items-center gap-2">
                <i className="pi pi-image" />
                <span>Hình ảnh & mô tả</span>
              </span>
            </Divider>
          </div>
          <div className="field col-12 md:col-8">
            <FloatLabel>
              <InputText
                id="imageUrl"
                value={form.imageUrl}
                maxLength={MAX_IMAGEURL}
                onChange={(e) => setForm({ ...form, imageUrl: e.target.value })}
                onBlur={(e) => setForm({ ...form, imageUrl: e.target.value.trim() })}
                placeholder="https://example.com/image.jpg"
                style={{ padding: "0.7rem" }}
              />
              <label htmlFor="imageUrl">Ảnh (URL)</label>
            </FloatLabel>
            <small className="text-600">Dán link http/https. Không hỗ trợ ảnh base64 (data:).</small>
          </div>
          <div className="field col-12 md:col-4">
            <Card subTitle={form.code || "—"} style={{ borderRadius: 8, boxShadow: "0 4px 10px rgba(0,0,0,0.1)" }}>
              <div className="flex align-items-center justify-content-center" style={{ height: 160, borderRadius: 8, overflow: "hidden" }}>
                <img
                  src={form.imageUrl || PLACEHOLDER_IMG}
                  alt="preview"
                  style={thumbStyle}
                  onError={(e) => (e.currentTarget.src = PLACEHOLDER_IMG)}
                />
              </div>
            </Card>
          </div>
          <div className="field col-12">
            <FloatLabel>
              <InputTextarea
                id="description"
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
                autoResize
                rows={3}
                style={{ padding: "0.7rem" }}
              />
              <label htmlFor="description">Mô tả</label>
            </FloatLabel>
          </div>

          {/* Preview */}
          <div className="col-12" style={{ marginTop: "1rem" }}>
            <Divider align="left" style={{ fontWeight: "600", fontSize: "1.1rem", color: "#555" }}>
              <span className="flex align-items-center gap-2">
                <i className="pi pi-eye" />
                <span>Preview</span>
              </span>
            </Divider>
          </div>
          <div className="col-12">
            <Card style={{ borderRadius: 12, boxShadow: "0 6px 18px rgba(0,0,0,0.1)" }}>
              <div className="flex flex-column md:flex-row gap-4 align-items-start">
                <div style={{ minWidth: 160, borderRadius: 12, overflow: "hidden" }}>
                  <img
                    src={form.imageUrl || PLACEHOLDER_IMG}
                    alt="preview"
                    style={thumbStyle}
                    onError={(e) => (e.currentTarget.src = PLACEHOLDER_IMG)}
                  />
                </div>
                <div className="flex flex-column gap-2">
                  <div className="text-xl font-semibold" style={{ color: "#222" }}>
                    {form.name || "Tên sản phẩm"}
                  </div>
                  <div className="text-600" style={{ color: "#555" }}>
                    {form.code || "—"}
                  </div>
                  <div style={{ color: "#666" }}>
                    {(catOptions.find((c) => c.value === form.categoryId)?.label) || "Danh mục"}
                  </div>
                  <div style={{ fontWeight: "600", color: "#444" }}>
                    {(Number(form.price) || 0).toLocaleString(undefined, { style: "currency", currency: "USD" })}
                  </div>
                  <div>
                    <Tag
                      value={form.isActive ? "Active" : "Inactive"}
                      severity={form.isActive ? "success" : "secondary"}
                      style={{ fontWeight: "600" }}
                    />
                  </div>
                </div>
              </div>
            </Card>
          </div>
        </div>
      </Dialog>
    </div>
  );
}
