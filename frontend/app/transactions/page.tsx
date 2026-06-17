import { redirect } from "next/navigation";
import routes from "@/transactions/routes";

/**
 * Redirects to the transactions current page.
 */
const TransactionsIndexPage = function (): never {
  redirect(routes.current());
};

export const dynamic = "force-dynamic";
export default TransactionsIndexPage;
