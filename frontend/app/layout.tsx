import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import AccessDeniedView from "@/framework/auth/AccessDeniedView";
import { AppRouterCacheProvider } from "@mui/material-nextjs/v15-appRouter";
import ApplicationShell from "@/framework/navigation/ApplicationShell";
import DateLocalizationProvider from "@/framework/forms/DateLocalizationProvider";
import type { JSX } from "react";
import type { Metadata } from "next";
import { auth } from "@/auth";
import getCurrentApplicationUser from "@/framework/auth/currentApplicationUser";

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
const RootLayout = async function ({
  children,
}: Readonly<{
  children: JSX.Element;
}>): Promise<JSX.Element> {
  const session = await auth();
  const currentApplicationUser =
    session === null
      ? { accessDenied: false, user: null }
      : await getCurrentApplicationUser();
  const content = currentApplicationUser.accessDenied ? (
    <AccessDeniedView />
  ) : (
    <ApplicationShell
      applicationUser={currentApplicationUser.user}
      user={session?.user}
    >
      {children}
    </ApplicationShell>
  );

  return (
    <html lang="en">
      <body>
        <AppRouterCacheProvider>
          <DateLocalizationProvider>{content}</DateLocalizationProvider>
        </AppRouterCacheProvider>
      </body>
    </html>
  );
};

export default RootLayout;
export { metadata };
