import {
  appendRepeatedSearchParam,
  buildUrl,
  objectToSearchParams,
} from "@/framework/routes/helpers";
import type { FundGoalTrendsSearchParams } from "@/fund-goals/trends/FundGoalTrends";
import type { FundGoalWorkspaceSearchParams } from "@/fund-goals/workspace/FundGoalWorkspace";
import type { Route } from "next";

/**
 * Converts the provided Fund Goal Workspace Search Params into URL search params.
 */
const fundGoalWorkspaceSearchParamsToSearchParams = function (
  searchParams: FundGoalWorkspaceSearchParams,
): URLSearchParams {
  const { fundIds, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "fundIds", fundIds);

  return params;
};

/**
 * Converts the provided Fund Goal Trends Search Params into URL search params.
 */
const fundGoalTrendsSearchParamsToSearchParams = function (
  searchParams: FundGoalTrendsSearchParams,
): URLSearchParams {
  const { fundName, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "fundName", fundName);

  return params;
};

/**
 * Defines the routes for the Fund Goals section of the application.
 */
const routes = {
  trends: (searchParams: FundGoalTrendsSearchParams): Route =>
    buildUrl(
      "/goals/trends",
      fundGoalTrendsSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: FundGoalWorkspaceSearchParams): Route =>
    buildUrl(
      "/goals/workspace",
      fundGoalWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceDetail: (
    fundId: string,
    searchParams: FundGoalWorkspaceSearchParams,
  ): Route =>
    buildUrl(
      `/goals/workspace/${fundId}`,
      fundGoalWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
