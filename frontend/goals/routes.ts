import {
  appendRepeatedSearchParam,
  buildUrl,
  objectToSearchParams,
} from "@/framework/routes/helpers";
import type { GoalTrendsSearchParams } from "@/goals/trends/GoalTrends";
import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { Route } from "next";

const pathWithSearchParams = function (
  pathname: string,
  searchParams: URLSearchParams,
): Route {
  return buildUrl(pathname, searchParams);
};

const goalWorkspaceSearchParamsToSearchParams = function (
  searchParams: GoalWorkspaceSearchParams,
): URLSearchParams {
  const { fundIds, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "fundIds", fundIds);

  return params;
};

/**
 * App routes related to goals.
 */
const goalTrendsSearchParamsToSearchParams = function (
  searchParams: GoalTrendsSearchParams,
): URLSearchParams {
  const { goalType, fundName, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "goalType", goalType);
  appendRepeatedSearchParam(params, "fundName", fundName);

  return params;
};

const routes = {
  index: (searchParams: GoalTrendsSearchParams): Route =>
    pathWithSearchParams(
      "/goals/trends",
      goalTrendsSearchParamsToSearchParams(searchParams),
    ),
  trends: (searchParams: GoalTrendsSearchParams): Route =>
    pathWithSearchParams(
      "/goals/trends",
      goalTrendsSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: GoalWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/goals/workspace",
      goalWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceDetail: (
    fundId: string,
    searchParams: GoalWorkspaceSearchParams,
  ): Route =>
    pathWithSearchParams(
      `/goals/workspace/${fundId}`,
      goalWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
