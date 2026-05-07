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
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to goals.
 */
const routes = {
  create: (searchParams: CreateGoalViewSearchParams): Route =>
    `/goals/create?${objectToSearchParams(searchParams).toString()}` as Route,
  detail: (params: GoalViewParams, searchParams: GoalViewSearchParams): Route =>
    `/goals/${params.id}?${objectToSearchParams(searchParams).toString()}` as Route,
  update: (
    params: UpdateGoalViewParams,
    searchParams: UpdateGoalViewSearchParams,
  ): Route =>
    `/goals/${params.id}/update?${objectToSearchParams(searchParams).toString()}` as Route,
  delete: (
    params: DeleteGoalViewParams,
    searchParams: DeleteGoalViewSearchParams,
  ): Route =>
    `/goals/${params.id}/delete?${objectToSearchParams(searchParams).toString()}` as Route,
};

export default routes;
