import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import App from "@src/App";
import React from "react";
import ReactDom from "react-dom/client";
import { ensureNotNull } from "@framework/utils";

ReactDom.createRoot(
  ensureNotNull(
    document.getElementById("root"),
    "Unable to find 'root' element.",
  ),
).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
);
