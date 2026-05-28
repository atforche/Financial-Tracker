import type {
  DeleteGoalViewParams,
  DeleteGoalViewSearchParams,
} from "@/goals/DeleteGoalView";
import type { GoalViewParams, GoalViewSearchParams } from "@/goals/GoalView";
import type {
  UpdateGoalViewParams,
  UpdateGoalViewSearchParams,
} from "@/goals/UpdateGoalView";
import type { CreateGoalViewSearchParams } from "@/goals/CreateGoalView";
import type { GoalsViewSearchParams } from "@/goals/GoalsView";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to goals.
 */
const routes = {
  index: (searchParams: GoalsViewSearchParams): Route =>
    `/goals?${objectToSearchParams(searchParams).toString()}`,
  create: (searchParams: CreateGoalViewSearchParams): Route =>
    `/goals/create?${objectToSearchParams(searchParams).toString()}`,
  detail: (params: GoalViewParams, searchParams: GoalViewSearchParams): Route =>
    `/goals/${params.id}?${objectToSearchParams(searchParams).toString()}`,
  update: (
    params: UpdateGoalViewParams,
    searchParams: UpdateGoalViewSearchParams,
  ): Route =>
    `/goals/${params.id}/update?${objectToSearchParams(searchParams).toString()}`,
  delete: (
    params: DeleteGoalViewParams,
    searchParams: DeleteGoalViewSearchParams,
  ): Route =>
    `/goals/${params.id}/delete?${objectToSearchParams(searchParams).toString()}`,
};

export default routes;
