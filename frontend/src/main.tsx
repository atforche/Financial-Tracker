/**
 * @file Main entry point for the application's front end.
 */

import "./index.css";
import App from "./App.tsx";
import React from "react";
import ReactDom from "react-dom/client";
import { ensureNotNull } from "./utils.ts";

ReactDom.createRoot(
  ensureNotNull(
    document.getElementById("root"),
    "Unable to find 'root' element."
  )
).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
