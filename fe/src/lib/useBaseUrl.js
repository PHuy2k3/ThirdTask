import { useEffect, useState } from "react";
const KEY = "categoryapi_base";
export function useBaseUrl(defaultBase = "http://localhost:5088") {
    const [baseUrl, setBaseUrl] = useState(() => localStorage.getItem(KEY) || defaultBase);
    useEffect(() => localStorage.setItem(KEY, baseUrl), [baseUrl]);
    return { baseUrl, setBaseUrl };
}