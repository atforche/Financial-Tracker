import AccountWorkspaceSetupPage from "@/accounts/workspace/AccountWorkspaceSetupPage";
import type { JSX } from "react";

/**
 * Route for the account create page.
 */
const Page = function (): JSX.Element {
  return <AccountWorkspaceSetupPage mode="create" />;
};

export const dynamic = "force-dynamic";
export default Page;
