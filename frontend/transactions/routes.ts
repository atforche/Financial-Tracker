import {
  appendRepeatedSearchParam,
  buildUrl,
  objectToSearchParams,
} from "@/framework/routes/helpers";
import type { Route } from "next";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";

/**
 * Converts the provided Transaction Workspace Search Params to URL Search Params.
 */
const transactionWorkspaceSearchParamsToSearchParams = function (
  searchParams: TransactionWorkspaceSearchParams,
): URLSearchParams {
  const {
    accountingPeriodIds,
    accountIds,
    fundIds,
    locationIds,
    fundNames,
    accountTypes,
    accountNames,
    transactionTypes,
    ...remainingSearchParams
  } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "accountingPeriodIds", accountingPeriodIds);
  appendRepeatedSearchParam(params, "accountIds", accountIds);
  appendRepeatedSearchParam(params, "fundIds", fundIds);
  appendRepeatedSearchParam(params, "locationIds", locationIds);
  appendRepeatedSearchParam(params, "fundNames", fundNames);
  appendRepeatedSearchParam(params, "accountTypes", accountTypes);
  appendRepeatedSearchParam(params, "accountNames", accountNames);
  appendRepeatedSearchParam(params, "transactionTypes", transactionTypes);
  return params;
};

/**
 * App routes related to accounting periods.
 */
const routes = {
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
