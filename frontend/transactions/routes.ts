import type { Route } from "next";
import type { TransactionTrendsSearchParams } from "@/transactions/trends/TransactionTrends";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import { appendRepeatedSearchParam } from "@/framework/routes/helpers";
import { objectToSearchParams } from "@/framework/routes";

const transactionTrendsSearchParamsToSearchParams = function (
  searchParams: TransactionTrendsSearchParams,
): URLSearchParams {
  const { transactionType, accountName, fundName, ...remainingSearchParams } =
    searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "transactionType", transactionType);
  appendRepeatedSearchParam(params, "accountName", accountName);
  appendRepeatedSearchParam(params, "fundName", fundName);
  return params;
};

const transactionWorkspaceSearchParamsToSearchParams = function (
  searchParams: TransactionWorkspaceSearchParams,
): URLSearchParams {
  const { accountingPeriodIds, accountIds, fundIds, ...remainingSearchParams } =
    searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "accountingPeriodIds", accountingPeriodIds);
  appendRepeatedSearchParam(params, "accountIds", accountIds);
  appendRepeatedSearchParam(params, "fundIds", fundIds);
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
  current: (): Route => "/transactions/current",
  trends: (searchParams: TransactionTrendsSearchParams): Route =>
    pathWithSearchParams(
      "/transactions/trends",
      transactionTrendsSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: TransactionWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/transactions/workspace",
      transactionWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceCreate: (searchParams: TransactionWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/transactions/workspace/create",
      transactionWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceDetail: (
    transactionId: string,
    searchParams: TransactionWorkspaceSearchParams,
  ): Route =>
    pathWithSearchParams(
      `/transactions/workspace/${transactionId}`,
      transactionWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceEdit: (
    transactionId: string,
    searchParams: TransactionWorkspaceSearchParams,
  ): Route =>
    pathWithSearchParams(
      `/transactions/workspace/${transactionId}/edit`,
      transactionWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
