import type { FundDashboardSearchParams } from "@/funds/dashboard/FundDashboard";
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

const fundDashboardSearchParamsToSearchParams = function (
  searchParams: FundDashboardSearchParams,
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
  dashboard: (searchParams: FundDashboardSearchParams): Route =>
    pathWithSearchParams(
      "/funds/dashboard",
      fundDashboardSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: FundWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/funds/workspace",
      objectToSearchParams(searchParams),
    ),
};

export default routes;
