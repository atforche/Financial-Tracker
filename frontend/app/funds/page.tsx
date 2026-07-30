import { redirect } from "next/navigation";
import routes from "@/funds/routes";

/**
 * Redirects to the funds workspace.
 */
const FundsIndexPage = function (): never {
  redirect(routes.workspace({}));
};

export const dynamic = "force-dynamic";
export default FundsIndexPage;
