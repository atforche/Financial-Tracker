import {
  appendRepeatedSearchParam,
  buildUrl,
  objectToSearchParams,
} from "@/framework/routes/helpers";
import type { FundPlanTrendsSearchParams } from "@/fund-plans/trends/FundPlanTrends";
import type { FundPlanWorkspaceSearchParams } from "@/fund-plans/workspace/FundPlanWorkspace";
import type { Route } from "next";

/**
 * Converts the provided Funding Plan Workspace Search Params into URL search params.
 */
const fundPlanWorkspaceSearchParamsToSearchParams = function (
  searchParams: FundPlanWorkspaceSearchParams,
): URLSearchParams {
  const { fundIds, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "fundIds", fundIds);

  return params;
};

/**
 * Converts the provided Funding Plan Trends Search Params into URL search params.
 */
const fundPlanTrendsSearchParamsToSearchParams = function (
  searchParams: FundPlanTrendsSearchParams,
): URLSearchParams {
  const { fundName, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "fundName", fundName);

  return params;
};

/**
 * Defines the routes for the Funding Plans section of the application.
 */
const routes = {
  trends: (searchParams: FundPlanTrendsSearchParams): Route =>
    buildUrl(
      "/goals/trends",
      fundPlanTrendsSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: FundPlanWorkspaceSearchParams): Route =>
    buildUrl(
      "/goals/workspace",
      fundPlanWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceDetail: (
    fundId: string,
    searchParams: FundPlanWorkspaceSearchParams,
  ): Route =>
    buildUrl(
      `/goals/workspace/${fundId}`,
      fundPlanWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
