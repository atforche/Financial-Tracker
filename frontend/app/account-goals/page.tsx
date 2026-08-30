import { redirect } from "next/navigation";
import routes from "@/account-goals/routes";

/**
 * Redirects to the Account Goals workspace page.
 */
const AccountGoalsIndexPage = function (): never {
  redirect(routes.workspace({}));
};

export const dynamic = "force-dynamic";
export default AccountGoalsIndexPage;
