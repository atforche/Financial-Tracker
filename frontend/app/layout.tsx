import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import { AppRouterCacheProvider } from "@mui/material-nextjs/v15-appRouter";
import type { JSX } from "react";
import type { Metadata } from "next";

/**
 * Metadata for the application, including title and description.
 */
const metadata: Metadata = {
  title: "Financial Tracker",
  description: "A comprehensive financial tracking application",
};

/**
 * Component that displays the main layout for the application.
 */
const RootLayout = function ({
  children,
}: Readonly<{
  children: JSX.Element;
}>): JSX.Element {
  return (
    <html lang="en">
      <body>
        <AppRouterCacheProvider>{children}</AppRouterCacheProvider>
      </body>
    </html>
  );
};

export default RootLayout;
export { metadata };
