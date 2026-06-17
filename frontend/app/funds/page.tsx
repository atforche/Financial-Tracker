import { redirect } from "next/navigation";
import routes from "@/funds/routes";

/**
 * Redirects to the funds trends.
 */
const FundsIndexPage = function (): never {
  redirect(routes.trends({}));
};

export const dynamic = "force-dynamic";
export default FundsIndexPage;
