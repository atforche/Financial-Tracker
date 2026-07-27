import { redirect } from "next/navigation";
import routes from "@/transactions/routes";

/**
 * Redirects to the transactions workspace.
 */
const TransactionsIndexPage = function (): never {
  redirect(routes.workspace({}));
};

export const dynamic = "force-dynamic";
export default TransactionsIndexPage;
