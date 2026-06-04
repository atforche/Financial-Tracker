import type { DeleteGoalViewParams } from "@/goals/DeleteGoalView";
import type { GoalsViewSearchParams } from "@/goals/GoalsView";
import type { Route } from "next";
import type { UpdateGoalViewParams } from "@/goals/UpdateGoalView";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to goals.
 */
const routes = {
  index: (searchParams: GoalsViewSearchParams): Route =>
    `/goals?${objectToSearchParams(searchParams).toString()}`,
  create: (): Route => `/goals/create`,
  update: (params: UpdateGoalViewParams): Route => `/goals/${params.id}/update`,
  delete: (params: DeleteGoalViewParams): Route => `/goals/${params.id}/delete`,
};

export default routes;
