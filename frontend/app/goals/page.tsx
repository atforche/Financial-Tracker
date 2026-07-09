import { redirect } from "next/navigation";
import routes from "@/goals/routes";

/**
 * Redirects to the goals workspace page.
 */
const GoalsIndexPage = function (): never {
  redirect(routes.workspace({}));
};

export const dynamic = "force-dynamic";
export default GoalsIndexPage;
