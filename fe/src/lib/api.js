// src/lib/api.js
import axios from "axios";

const ENV_BASE =
  (import.meta?.env && import.meta.env.VITE_API_BASE) ||
  (typeof process !== "undefined" && process.env.REACT_APP_API_BASE) ||
  "http://localhost:5088";

export function normalizeBase(raw) {
  const s = (raw || "").trim().replace(/\/+$/, "");
  try {
    const u = new URL(s || ENV_BASE);
    if (u.hostname === "localhost" && u.port === "5088") u.protocol = "http:";
    return u.toString().replace(/\/+$/, "");
  } catch {
    return (s || ENV_BASE).replace(/\/+$/, "");
  }
}

export function getAxiosMessage(err, fallback = "Request failed") {
  if (!err) return fallback;
  return (
    err?.response?.data?.message ||
    err?.response?.data?.error ||
    err?.response?.data?.title ||
    err?.response?.data?.detail ||
    err?.message ||
    fallback
  );
}

export const buildQuery = (obj) => {
  const q = Object.entries(obj ?? {})
    .filter(([, v]) => v !== undefined && v !== null && v !== "")
    .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v)}`)
    .join("&");
  return q ? `?${q}` : "";
};


let apiLogger = (evt) => {
  if (evt.type === "error") {
    const { method, url, status, message, code, data } = evt;
    console.error(`[API ERR] ${method} ${url}${status ? ` ${status}` : ""}`, message, { code, data });
  } else if (evt.type === "success") {
    const { status, url, duration } = evt;
    console.log(`[API OK] ${status} ${url} (${duration}ms)`);
  } else if (evt.type === "request") {
    const { method, url, params } = evt;
    console.log(`[API REQ] ${method} ${url}`, params || "");
  }
};
export function setApiLogger(fn) {
  if (typeof fn === "function") apiLogger = fn;
}

function ensureApiPath(path) {
  if (!/^\/api\//.test(path)) {
    throw new Error(`Path must start with /api/: ${path}`);
  }
  return path;
}

export function createApi(baseUrl, options = {}) {
  const base = normalizeBase(baseUrl || ENV_BASE);
  const instance = axios.create({
    baseURL: base,
    timeout: options.timeout ?? 15000,
    withCredentials: false,
    headers: { "Content-Type": "application/json" },
  });

  const enableLog =
    options.log ?? (typeof window !== "undefined" && process.env.NODE_ENV !== "production");

  if (enableLog) {
    instance.interceptors.request.use((config) => {
      const method = (config.method || "get").toUpperCase();
      const url = (config.baseURL || "") + (config.url || "");
      apiLogger({ type: "request", method, url, params: config.params });
      config.metadata = { start: (performance?.now?.() || Date.now()) };
      return config;
    });

    instance.interceptors.response.use(
      (res) => {
        const start = res.config?.metadata?.start ?? Date.now();
        const duration = Math.round((performance?.now?.() || Date.now()) - start);
        const url = (res.config.baseURL || "") + (res.config.url || "");
        apiLogger({ type: "success", status: res.status, url, duration });
        return res;
      },
      (err) => {
        const cfg = err.config || {};
        const url = (cfg.baseURL || "") + (cfg.url || "");
        const method = (cfg.method || "get").toUpperCase();
        const status = err.response?.status;
        apiLogger({
          type: "error",
          method,
          url,
          status,
          message: getAxiosMessage(err),
          code: err.code,
          data: err.response?.data,
        });
        return Promise.reject(err);
      }
    );
  }

  return {
    raw: instance,
    get: (path, config) => instance.get(ensureApiPath(path), config),
    post: (path, data, config) => instance.post(ensureApiPath(path), data, config),
    put: (path, data, config) => instance.put(ensureApiPath(path), data, config),
    delete: (path, config) => instance.delete(ensureApiPath(path), config),
  };
}

export async function testConnection(baseUrl) {
  const api = createApi(baseUrl, { log: false });
  const candidates = ["/api/health", "/swagger/v1/swagger.json", "/api/categories"];
  for (const p of candidates) {
    try {
      const r = await api.get(p);
      if (r?.status >= 200 && r?.status < 500) {
        return { ok: true, url: api.raw.defaults.baseURL + p };
      }
    } catch {}
  }
  return { ok: false, baseUrl: api.raw.defaults.baseURL, error: "Cannot reach API" };
}
