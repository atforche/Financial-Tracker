import {
  appendRepeatedSearchParam,
  buildUrl,
  objectToSearchParams,
} from "@/framework/routes/helpers";
import type { AccountTrendsSearchParams } from "@/accounts/trends/helpers";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/types";
import type { Route } from "next";

/**
 * Converts the provided Account Trends Search Params to URL search params for the account workspace.
 */
const accountTrendsSearchParamsToSearchParams = function (
  searchParams: AccountTrendsSearchParams,
): URLSearchParams {
  const { accountType, accountName, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "accountType", accountType);
  appendRepeatedSearchParam(params, "accountName", accountName);
  return params;
};

/**
 * Converts the provided Account Workspace Search Params to URL search params for the account workspace.
 */
const accountWorkspaceSearchParamsToSearchParams = function (
  searchParams: AccountWorkspaceSearchParams,
): URLSearchParams {
  const { accountType, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "accountType", accountType);
  return params;
};

/**
 * App routes related to accounts.
 */
const routes = {
  trends: (searchParams: AccountTrendsSearchParams): Route =>
    buildUrl(
      "/accounts/trends",
      accountTrendsSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: AccountWorkspaceSearchParams): Route =>
    buildUrl(
      "/accounts/workspace",
      accountWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceDetail: (
    accountId: string,
    searchParams: AccountWorkspaceSearchParams,
  ): Route =>
    buildUrl(
      `/accounts/workspace/${accountId}`,
      accountWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
