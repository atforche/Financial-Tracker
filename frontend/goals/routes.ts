import {
  appendRepeatedSearchParam,
  buildUrl,
  objectToSearchParams,
} from "@/framework/routes/helpers";
import type { GoalTrendsSearchParams } from "@/goals/trends/GoalTrends";
import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { Route } from "next";

/**
 * Converts the provided Goal Workspace Search Params into URL search params.
 */
const goalWorkspaceSearchParamsToSearchParams = function (
  searchParams: GoalWorkspaceSearchParams,
): URLSearchParams {
  const { fundIds, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "fundIds", fundIds);

  return params;
};

/**
 * Converts the provided Goal Trends Search Params into URL search params.
 */
const goalTrendsSearchParamsToSearchParams = function (
  searchParams: GoalTrendsSearchParams,
): URLSearchParams {
  const { fundName, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "fundName", fundName);

  return params;
};

/**
 * Defines the routes for the Goals section of the application.
 */
const routes = {
  trends: (searchParams: GoalTrendsSearchParams): Route =>
    buildUrl(
      "/goals/trends",
      goalTrendsSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: GoalWorkspaceSearchParams): Route =>
    buildUrl(
      "/goals/workspace",
      goalWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceDetail: (
    fundId: string,
    searchParams: GoalWorkspaceSearchParams,
  ): Route =>
    buildUrl(
      `/goals/workspace/${fundId}`,
      goalWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
