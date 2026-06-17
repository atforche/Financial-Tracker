import { redirect } from "next/navigation";
import routes from "@/goals/routes";

/**
 * Redirects to the goals current page.
 */
const GoalsIndexPage = function (): never {
  redirect(routes.current());
};

export const dynamic = "force-dynamic";
export default GoalsIndexPage;
