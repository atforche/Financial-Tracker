import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";

/**
 * Redirects to the accounting periods trends.
 */
const AccountingPeriodsIndexPage = function (): never {
  redirect(routes.current({}));
};

export const dynamic = "force-dynamic";
export default AccountingPeriodsIndexPage;
