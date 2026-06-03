import { redirect } from "next/navigation";
import routes from "@/funds/routes";

/**
 * Redirects to the funds dashboard.
 */
const FundsIndexPage = function (): never {
  redirect(routes.dashboard({}));
};

export const dynamic = "force-dynamic";
export default FundsIndexPage;
