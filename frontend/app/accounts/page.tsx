import { redirect } from "next/navigation";
import routes from "@/accounts/routes";

/**
 * Redirects to the accounts current page.
 */
const AccountsIndexPage = function (): never {
  redirect(routes.workspace({}));
};

export const dynamic = "force-dynamic";
export default AccountsIndexPage;
