import type { Route } from "next";
import type { TransactionDashboardSearchParams } from "@/transactions/dashboard/TransactionDashboard";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
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

const transactionDashboardSearchParamsToSearchParams = function (
  searchParams: TransactionDashboardSearchParams,
): URLSearchParams {
  const { transactionType, accountName, fundName, ...remainingSearchParams } =
    searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "transactionType", transactionType);
  appendRepeatedSearchParam(params, "accountName", accountName);
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
 * App routes related to accounting periods.
 */
const routes = {
  dashboard: (searchParams: TransactionDashboardSearchParams): Route =>
    pathWithSearchParams(
      "/transactions/dashboard",
      transactionDashboardSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: TransactionWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/transactions/workspace",
      objectToSearchParams(searchParams),
    ),
};

export default routes;
