import { redirect } from "next/navigation";
import routes from "@/accounts/routes";

/**
 * Redirects to the accounts trends.
 */
const AccountsIndexPage = function (): never {
  redirect(routes.trends({}));
};

export const dynamic = "force-dynamic";
export default AccountsIndexPage;
