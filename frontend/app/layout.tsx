import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import { Box, Stack } from "@mui/material";
import { AppRouterCacheProvider } from "@mui/material-nextjs/v15-appRouter";
import type { JSX } from "react";
import type { Metadata } from "next";
import Navigation from "@/app/Navigation";

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
        <AppRouterCacheProvider>
          <Stack direction="row">
            <Navigation />
            <Box sx={{ padding: "25px", width: "100%" }}>{children}</Box>
          </Stack>
        </AppRouterCacheProvider>
      </body>
    </html>
  );
};

export default RootLayout;
export { metadata };
