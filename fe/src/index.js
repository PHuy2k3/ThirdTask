import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import App from "./App.js";


// PrimeReact styles
import "primereact/resources/themes/lara-light-blue/theme.css"; // có thể đổi sang lara-dark-blue
import "primereact/resources/primereact.min.css";
import "primeicons/primeicons.css";


// PrimeReact provider (bật ripple, input outlined)
import { PrimeReactProvider } from "primereact/api";


// Custom styles
import "./styles.css";


ReactDOM.createRoot(document.getElementById("root")).render(
    <PrimeReactProvider value={{ ripple: true, inputStyle: "outlined" }}>
        <BrowserRouter>
            <App />
        </BrowserRouter>
    </PrimeReactProvider>
);