import type { AccountTrendsSearchParams } from "@/accounts/trends/AccountTrends";
import type { AccountType } from "@/accounts/types";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/AccountWorkspace";
import type { Route } from "next";
import { appendRepeatedSearchParam } from "@/framework/routes/helpers";
import { objectToSearchParams } from "@/framework/routes";

const isAccountTypeArray = function (
  value: AccountTrendsSearchParams["accountType"],
): value is readonly AccountType[] {
  return Array.isArray(value);
};

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

const pathWithSearchParams = function (
  pathname: string,
  searchParams: URLSearchParams,
): Route {
  const query = searchParams.toString();
  return query === "" ? pathname : `${pathname}?${query}`;
};

/**
 * App routes related to accounts.
 */
const routes = {
  trends: (searchParams: AccountTrendsSearchParams): Route =>
    pathWithSearchParams(
      "/accounts/trends",
      accountTrendsSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: AccountWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/accounts/workspace",
      accountWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceDetail: (
    accountId: string,
    searchParams: AccountWorkspaceSearchParams,
  ): Route =>
    pathWithSearchParams(
      `/accounts/workspace/${accountId}`,
      accountWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
