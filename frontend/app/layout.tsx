import type { Metadata } from "next";
import { AppRouterCacheProvider } from "@mui/material-nextjs/v15-appRouter";
import Navigation from "@/app/Navigation";
import Stack from "@mui/material/Stack";

export const metadata: Metadata = {
  title: "Financial Tracker",
  description: "A comprehensive financial tracking application",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>
        <AppRouterCacheProvider>
          <Stack direction="row">
            <Navigation />
            {children}
          </Stack>
        </AppRouterCacheProvider>
      </body>
    </html>
  );
}
