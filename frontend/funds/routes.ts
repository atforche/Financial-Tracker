import {
  appendRepeatedSearchParam,
  buildUrl,
  objectToSearchParams,
} from "@/framework/routes/helpers";
import type { FundTrendsSearchParams } from "@/funds/trends/helpers";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/types";
import type { Route } from "next";

/**
 * Converts the provided Fund Trends Search Params to URL search params.
 */
const fundTrendsSearchParamsToSearchParams = function (
  searchParams: FundTrendsSearchParams,
): URLSearchParams {
  const { fundName, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);
  appendRepeatedSearchParam(params, "fundName", fundName);
  return params;
};

/**
 * App routes related to funds.
 */
const routes = {
  trends: (searchParams: FundTrendsSearchParams): Route =>
    buildUrl(
      "/funds/trends",
      fundTrendsSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: FundWorkspaceSearchParams): Route =>
    buildUrl("/funds/workspace", objectToSearchParams(searchParams)),
  workspaceDetail: (
    fundId: string,
    searchParams: FundWorkspaceSearchParams,
  ): Route =>
    buildUrl(`/funds/workspace/${fundId}`, objectToSearchParams(searchParams)),
  workspaceCreate: (searchParams: FundWorkspaceSearchParams): Route =>
    buildUrl("/funds/workspace/create", objectToSearchParams(searchParams)),
  workspaceOnboard: (searchParams: FundWorkspaceSearchParams): Route =>
    buildUrl("/funds/workspace/onboard", objectToSearchParams(searchParams)),
};

export default routes;
