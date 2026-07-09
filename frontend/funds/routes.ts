import type { FundTrendsSearchParams } from "@/funds/trends/FundTrends";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/FundWorkspace";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

const isRepeatedSearchParamArray = function (
  value: string | readonly string[] | undefined,
): value is readonly string[] {
  return Array.isArray(value);
};

const appendRepeatedSearchParam = function (
  params: URLSearchParams,
  key: string,
  value: string | readonly string[] | undefined,
): void {
  if (isRepeatedSearchParamArray(value)) {
    value.forEach((item) => {
      params.append(key, item);
    });
    return;
  }
  if (typeof value === "string") {
    params.append(key, value);
  }
};

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
  current: (): Route => "/funds/current",
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
