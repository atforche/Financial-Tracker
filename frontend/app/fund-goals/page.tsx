import { redirect } from "next/navigation";
import routes from "@/fund-goals/routes";

/**
 * Redirects to the Fund Goals workspace page.
 */
const FundGoalsIndexPage = function (): never {
  redirect(routes.workspace({}));
};

export const dynamic = "force-dynamic";
export default FundGoalsIndexPage;
