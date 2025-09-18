import React, { useRef } from "react";
import { Routes, Route, Navigate, useLocation, useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { Toast } from "primereact/toast";
import { BreadCrumb } from "primereact/breadcrumb";
import { TabMenu } from "primereact/tabmenu";
import TopBar from "./components/topbar.js";
import CategoriesPage from "./pages/Categories.js";
import CatalogsPage from "./pages/Catalogs.js";
import CategoryCatalogPage from "./pages/CategoryCatalog.js";
import { useBaseUrl } from "./lib/useBaseUrl.js";


export default function App() {
const toast = useRef(null);
const { baseUrl, setBaseUrl } = useBaseUrl();
const onToast = (severity, summary, detail) => toast.current?.show({ severity, summary, detail, life: 3000 });


const loc = useLocation();
const nav = useNavigate();


const tabs = [
{ label: "Combined", icon: "pi pi-table", to: "/combined" },
{ label: "Categories", icon: "pi pi-list", to: "/categories" },
{ label: "Catalogs", icon: "pi pi-box", to: "/catalogs" },
];
const activeIndex = Math.max(0, tabs.findIndex(t => loc.pathname.startsWith(t.to)));


const breadcrumb = {
  model: loc.pathname.split("/").filter(Boolean).map((seg, idx, arr) => ({
  label: seg.charAt(0).toUpperCase() + seg.slice(1),
  command: () => nav("/" + arr.slice(0, idx + 1).join("/"))
  }))
};


return (
  <div className="app-shell">
    <Toast ref={toast} />
    <TopBar baseUrl={baseUrl} setBaseUrl={setBaseUrl} onToast={onToast} />


    <div className="mb-3 flex align-items-center justify-content-between">
      <BreadCrumb home={breadcrumb.home} model={breadcrumb.model} className="surface-card border-round-2xl shadow-soft px-3 py-2" />
    </div>


    <div className="mb-3">
      <TabMenu model={tabs} activeIndex={activeIndex} onTabChange={(e) => nav(tabs[e.index].to)} className="surface-card border-round-2xl shadow-soft" />
    </div>


    <Card className="card-rounded shadow-soft">
      <Routes>
      <Route path="/" element={<Navigate to="/combined" replace />} />
      <Route path="/categories" element={<CategoriesPage baseUrl={baseUrl} onToast={onToast} />} />
      <Route path="/catalogs" element={<CatalogsPage baseUrl={baseUrl} onToast={onToast} />} />
      <Route path="/combined" element={<CategoryCatalogPage baseUrl={baseUrl} onToast={onToast} />} />
      <Route path="*" element={<div className="p-4 text-600">Không tìm thấy trang.</div>} />
      </Routes>
    </Card>
  </div>
);
}