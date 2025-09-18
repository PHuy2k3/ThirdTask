import React from "react";
import { Toolbar } from "primereact/toolbar";
import { InputText } from "primereact/inputtext";
import { Button } from "primereact/button";
import { Badge } from "primereact/badge";
import { testConnection } from "../lib/api";

export default function TopBar({ baseUrl, setBaseUrl, onToast }) {
  const left = (
    <div className="flex gap-3 align-items-center">
      <i className="pi pi-box text-primary text-xl" />
      <div className="flex flex-column">
        <span className="font-semibold text-900">Category Admin</span>
        <small className="text-600">• .NET 9 API</small>
      </div>
      <Badge value="v1" severity="info" className="ml-2" />
    </div>
  );

  const right = (
    <div className="flex align-items-center gap-2">
      <span className="p-input-icon-left">
        <InputText value={baseUrl} onChange={(e) => setBaseUrl(e.target.value)} placeholder="Base URL" className="w-20rem" />
      </span>
      <Button icon="pi pi-check" label="Test" onClick={async () => {
        try { await testConnection(baseUrl); onToast("success", "Kết nối OK", baseUrl); }
        catch (err) { onToast("error", "Kết nối lỗi", err.message); }
      }} />
    </div>
  );

  return (
    <div className="sticky-header">
      <Toolbar className="mb-3 surface-card border-round-2xl shadow-soft" left={left} right={right} />
    </div>
  );
}