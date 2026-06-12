import type { AccountingPeriodDashboardSearchParams } from "@/accounting-periods/dashboard/AccountingPeriodDashboard";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

const isRepeatedSearchParamArray = function (
  value: number | readonly number[] | undefined,
): value is readonly number[] {
  return Array.isArray(value);
};

const appendRepeatedSearchParam = function (
  params: URLSearchParams,
  key: string,
  value: number | readonly number[] | undefined,
): void {
  if (isRepeatedSearchParamArray(value)) {
    value.forEach((item) => {
      params.append(key, String(item));
    });
    return;
  }

  if (typeof value === "number") {
    params.append(key, String(value));
  }
};

const accountingPeriodWorkspaceSearchParamsToSearchParams = function (
  searchParams: AccountingPeriodWorkspaceSearchParams,
): URLSearchParams {
  const { years, months, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "years", years);
  appendRepeatedSearchParam(params, "months", months);

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
 * App routes related to accounting periods.
 */
const routes = {
  dashboard: (searchParams: AccountingPeriodDashboardSearchParams): Route =>
    pathWithSearchParams(
      "/accounting-periods/dashboard",
      objectToSearchParams(searchParams),
    ),
  workspace: (searchParams: AccountingPeriodWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/accounting-periods/workspace",
      accountingPeriodWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
