import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";

/**
 * Redirects to the accounting periods workspace.
 */
const AccountingPeriodsIndexPage = function (): never {
  redirect(routes.workspace({}));
};

export const dynamic = "force-dynamic";
export default AccountingPeriodsIndexPage;
