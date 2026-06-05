import { redirect } from "next/navigation";
import routes from "@/transactions/routes";

/**
 * Redirects to the transactions dashboard.
 */
const TransactionsIndexPage = function (): never {
  redirect(routes.dashboard({}));
};

export const dynamic = "force-dynamic";
export default TransactionsIndexPage;
