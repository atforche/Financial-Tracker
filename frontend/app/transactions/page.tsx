import { redirect } from "next/navigation";
import routes from "@/transactions/routes";

/**
 * Redirects to the transactions trends.
 */
const TransactionsIndexPage = function (): never {
  redirect(routes.trends({}));
};

export const dynamic = "force-dynamic";
export default TransactionsIndexPage;
