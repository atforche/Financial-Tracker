import { redirect } from "next/navigation";
import routes from "@/funds/routes";

/**
 * Redirects to the funds current page.
 */
const FundsIndexPage = function (): never {
  redirect(routes.current());
};

export const dynamic = "force-dynamic";
export default FundsIndexPage;
