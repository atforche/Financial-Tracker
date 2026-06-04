import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";

/**
 * Redirects to the accounting periods dashboard.
 */
const AccountingPeriodsIndexPage = function (): never {
  redirect(routes.dashboard({}));
};

export const dynamic = "force-dynamic";
export default AccountingPeriodsIndexPage;
