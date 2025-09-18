// src/pages/Categories.js
import React, { useEffect, useMemo, useState } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Dialog } from "primereact/dialog";
import { InputText } from "primereact/inputtext";
import { InputNumber } from "primereact/inputnumber";
import { InputSwitch } from "primereact/inputswitch";
import { Button } from "primereact/button";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import { Tag } from "primereact/tag";
import { createApi, getAxiosMessage } from "../lib/api";

export default function CategoriesPage({ baseUrl, onToast }) {
  const api = useMemo(() => createApi(baseUrl), [baseUrl]);

  const [q, setQ] = useState("");
  const [page, setPage] = useState(1);
  const [size, setSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(false);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editId, setEditId] = useState(null);
  const [form, setForm] = useState({ name: "", parentId: null, sortOrder: 0, isActive: true });

  const loadingData = async () => {
    setLoading(true);
    try {
      const { data } = await api.get("/api/categories", { params: { page, size, q } });
      setRows(data?.items ?? []);
      setTotal(data?.total ?? 0);
    } catch (err) {
      onToast("error", "Tải categories thất bại", getAxiosMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadingData();
    // eslint-disable-next-line
  }, [page, size]);

  const onSearch = () => {
    setPage(1);
    loadingData();
  };

  const onClear = () => {
    setQ("");
    setPage(1);
    loadingData();
  };

  const openNew = () => {
    setEditId(null);
    setForm({ name: "", parentId: null, sortOrder: 0, isActive: true });
    setDialogOpen(true);
  };

  const openEdit = (r) => {
    setEditId(r.id);
    setForm({
      name: r.name,
      parentId: r.parentId ?? null,
      sortOrder: r.sortOrder ?? 0,
      isActive: !!r.isActive,
    });
    setDialogOpen(true);
  };

  const save = async () => {
    try {
      if (!form.name?.trim()) return onToast("warn", "Thiếu dữ liệu", "Tên danh mục bắt buộc");
      if (editId == null) {
        await api.post("/api/categories", form);
        onToast("success", "Đã tạo", form.name);
      } else {
        await api.put(`/api/categories/${editId}`, form);
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
      message: `Xoá danh mục "${r.name}"?`,
      header: "Xác nhận",
      icon: "pi pi-exclamation-triangle",
      acceptClassName: "p-button-danger",
      accept: async () => {
        try {
          await api.delete(`/api/categories/${r.id}`);
          onToast("success", "Đã xoá", r.name);
          loadingData();
        } catch (err) {
          onToast("error", "Xoá thất bại", getAxiosMessage(err));
        }
      },
    });

  return (
    <div className="flex flex-column gap-3">
      <ConfirmDialog />
      <div
        className="surface-card border-round-xl p-3 flex align-items-center justify-content-between flex-wrap gap-2 shadow-1"
      >
        <div className="flex align-items-center gap-2" style={{ flex: 1, maxWidth: 680 }}>
          <span className="p-input-icon-left p-input-icon-right w-full">
            <InputText
              value={q}
              onChange={(e) => setQ(e.target.value)}
              placeholder="Tìm kiếm"
              onKeyDown={(e) => e.key === "Enter" && onSearch()}
              className="w-full"
            />
            {q && (
              <i
                className="pi pi-times cursor-pointer"
                onClick={onClear}
                title="Xoá lọc"
                aria-label="Xoá lọc"
              />
            )}
          </span>
          <Button icon="pi pi-search" label="Tìm" onClick={onSearch} style={{ marginRight: 10, marginLeft: 30 }} />
          <Button icon="pi pi-plus" label="Thêm" onClick={openNew} />
        </div>
      </div>

      <DataTable
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
        paginatorTemplate="RowsPerPageDropdown FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
        currentPageReportTemplate="({first}–{last} / {totalRecords})"
        emptyMessage="Không có dữ liệu"
        showGridlines
        rowHover
        stripedRows
        responsiveLayout="scroll"
        className="shadow-soft card-rounded"
      >
        <Column field="id" header="#" style={{ width: 80 }}/>
        <Column field="name" header="Tên" />
        <Column field="slug" header="Slug" />
        <Column field="parentId" header="ParentId" style={{ width: 110 }} />
        <Column field="sortOrder" header="Thứ tự" style={{ width: 110 }} />
        <Column
          header="Trạng thái"
          body={(r) => (
            <Tag value={r.isActive ? "Active" : "Inactive"} severity={r.isActive ? "success" : "secondary"} rounded />
          )}
          style={{ width: 140 }}
        />
        <Column
          header="Thao tác"
          style={{ width: 170 }}
          body={(r) => (
            <div className="flex gap-1">
              <Button
                icon="pi pi-pencil"
                className="p-button-rounded p-button-text p-button-sm"
                onClick={() => openEdit(r)}
                tooltip="Sửa"
              />
              <Button
                icon="pi pi-trash"
                className="p-button-rounded p-button-text p-button-danger p-button-sm"
                onClick={() => remove(r)}
                tooltip="Xoá"
              />
            </div>
          )}
        />
      </DataTable>

      <Dialog
        header={editId == null ? "Thêm danh mục" : "Sửa danh mục"}
        visible={dialogOpen}
        modal
        onHide={() => setDialogOpen(false)}
        style={{ width: "48rem", maxWidth: "95vw" }}
        breakpoints={{ "960px": "70vw", "640px": "95vw" }}
        className="card-rounded"
        footer={
          <div className="flex justify-content-end gap-2">
            <Button label="Huỷ" className="p-button-text" onClick={() => setDialogOpen(false)} />
            <Button label="Lưu" icon="pi pi-save" onClick={save} />
          </div>
        }
      >
        <div className="p-fluid grid formgrid">
          <div className="col-12">
            <div className="field">
              <label style={{ fontWeight: 700, display: "block", marginBottom: 4 }}>Tên *</label>
              <InputText
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                placeholder="Nhập tên danh mục"
              />
            </div>
          </div>

          <div className="field col-12 md:col-6">
            <label style={{ fontWeight: 700, display: "block", marginBottom: 4 }}>Parent Id</label>
            <InputNumber
              inputClassName="w-full"
              value={form.parentId}
              onValueChange={(e) => setForm({ ...form, parentId: e.value })}
              showButtons
              step={1}
              min={0}
              placeholder="null"
              useGrouping={false}
            />
            <small className="text-600">Để trống (null) nếu là danh mục gốc.</small>
          </div>

          <div className="field col-12 md:col-6">
            <label style={{ fontWeight: 700, display: "block", marginBottom: 4 }}>Thứ tự</label>
            <InputNumber
              inputClassName="w-full"
              value={form.sortOrder}
              onValueChange={(e) => setForm({ ...form, sortOrder: e.value ?? 0 })}
              showButtons
              step={1}
              useGrouping={false}
            />
            <small className="text-600">Dùng để sắp xếp hiển thị.</small>
          </div>

          <div className="field col-12">
            <div className="flex align-items-center gap-2">
              <InputSwitch
                checked={form.isActive}
                onChange={(e) => setForm({ ...form, isActive: e.value })}
              />
              <span style={{ color: "#6b7280" }}>{form.isActive ? "Đang bật" : "Đang tắt"}</span>
            </div>
          </div>
        </div>
      </Dialog>
    </div>
  );
}
