import { redirect } from "next/navigation";
import routes from "@/accounts/routes";

/**
 * Redirects to the accounts dashboard.
 */
const AccountsIndexPage = function (): never {
  redirect(routes.dashboard({}));
};

export const dynamic = "force-dynamic";
export default AccountsIndexPage;
