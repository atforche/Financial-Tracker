import type {
  DeleteGoalViewParams,
  DeleteGoalViewSearchParams,
} from "@/goals/delete/DeleteGoalView";
import type {
  GoalDetailViewParams,
  GoalDetailViewSearchParams,
} from "@/goals/detail/GoalDetailView";
import type {
  UpdateGoalViewParams,
  UpdateGoalViewSearchParams,
} from "@/goals/update/UpdateGoalView";
import type { CreateGoalViewSearchParams } from "@/goals/create/CreateGoalView";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to goals.
 */
const routes = {
  create: (searchParams: CreateGoalViewSearchParams): Route =>
    `/goals/create?${objectToSearchParams(searchParams).toString()}` as Route,
  detail: (
    params: GoalDetailViewParams,
    searchParams: GoalDetailViewSearchParams,
  ): Route =>
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
