import {
  appendRepeatedSearchParam,
  buildUrl,
} from "@/framework/routes/helpers";
import type { AccountTrendsSearchParams } from "@/accounts/trends/AccountTrends";
import type { AccountType } from "@/accounts/types";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/types";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

/**
 * Determines if the provided value is an account type array.
 */
const isAccountTypeArray = function (
  value: AccountTrendsSearchParams["accountType"],
): value is readonly AccountType[] {
  return Array.isArray(value);
};

/**
 * Converts the provided Account Trends Search Params to URL search params for the account workspace.
 */
const accountTrendsSearchParamsToSearchParams = function (
  searchParams: AccountTrendsSearchParams,
): URLSearchParams {
  const { accountType, accountName, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(
    params,
    "accountType",
    isAccountTypeArray(accountType) ? accountType : accountType,
  );
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

  appendRepeatedSearchParam(
    params,
    "accountType",
    isAccountTypeArray(accountType) ? accountType : accountType,
  );
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
