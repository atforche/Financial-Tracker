import {
  appendRepeatedSearchParam,
  buildUrl,
  objectToSearchParams,
} from "@/framework/routes/helpers";
import type { Route } from "next";
import type { TransactionTrendsSearchParams } from "@/transactions/trends/TransactionTrends";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";

/**
 * Converts the provided Transaction Trends Search Params to URL Search Params.
 */
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

/**
 * Converts the provided Transaction Workspace Search Params to URL Search Params.
 */
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

/**
 * App routes related to accounting periods.
 */
const routes = {
  current: (): Route => "/transactions/current",
  trends: (searchParams: TransactionTrendsSearchParams): Route =>
    buildUrl(
      "/transactions/trends",
      transactionTrendsSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: TransactionWorkspaceSearchParams): Route =>
    buildUrl(
      "/transactions/workspace",
      transactionWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceCreate: (searchParams: TransactionWorkspaceSearchParams): Route =>
    buildUrl(
      "/transactions/workspace/create",
      transactionWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceDetail: (
    transactionId: string,
    searchParams: TransactionWorkspaceSearchParams,
  ): Route =>
    buildUrl(
      `/transactions/workspace/${transactionId}`,
      transactionWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceEdit: (
    transactionId: string,
    searchParams: TransactionWorkspaceSearchParams,
  ): Route =>
    buildUrl(
      `/transactions/workspace/${transactionId}/edit`,
      transactionWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
