import type { FundTrendsSearchParams } from "@/funds/trends/FundTrends";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/FundWorkspace";
import type { Route } from "next";
import { appendRepeatedSearchParam } from "@/framework/routes/helpers";
import { objectToSearchParams } from "@/framework/routes";

const fundTrendsSearchParamsToSearchParams = function (
  searchParams: FundTrendsSearchParams,
): URLSearchParams {
  const { fundName, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);
  appendRepeatedSearchParam(params, "fundName", fundName);
  return params;
};

const pathWithSearchParams = function (
  pathname: string,
  searchParams: URLSearchParams,
): Route {
  const query = searchParams.toString();
  return query === "" ? pathname : `${pathname}?${query}`;
};

/**
 * App routes related to funds.
 */
const routes = {
  trends: (searchParams: FundTrendsSearchParams): Route =>
    pathWithSearchParams(
      "/funds/trends",
      fundTrendsSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: FundWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/funds/workspace",
      objectToSearchParams(searchParams),
    ),
  workspaceDetail: (
    fundId: string,
    searchParams: FundWorkspaceSearchParams,
  ): Route =>
    pathWithSearchParams(
      `/funds/workspace/${fundId}`,
      objectToSearchParams(searchParams),
    ),
  workspaceCreate: (searchParams: FundWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/funds/workspace/create",
      objectToSearchParams(searchParams),
    ),
  workspaceOnboard: (searchParams: FundWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/funds/workspace/onboard",
      objectToSearchParams(searchParams),
    ),
};

export default routes;
