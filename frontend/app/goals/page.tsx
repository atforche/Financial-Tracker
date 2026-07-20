import { redirect } from "next/navigation";
import routes from "@/fund-plans/routes";

/**
 * Redirects to the Funding Plans workspace page.
 */
const FundPlansIndexPage = function (): never {
  redirect(routes.workspace({}));
};

export const dynamic = "force-dynamic";
export default FundPlansIndexPage;
