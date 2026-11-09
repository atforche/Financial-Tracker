import AccountWorkspaceSetupPage from "@/accounts/workspace/AccountWorkspaceSetupPage";
import type { JSX } from "react";

/**
 * Route for the account onboard page.
 */
const Page = function (): JSX.Element {
  return <AccountWorkspaceSetupPage mode="onboard" />;
};

export const dynamic = "force-dynamic";
export default Page;
