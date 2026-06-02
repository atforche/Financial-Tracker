import type { AccountDashboardSearchParams } from "@/accounts/dashboard/AccountDashboard";
import type { AccountType } from "@/accounts/types";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/AccountWorkspace";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

const isAccountTypeArray = function (
  value: AccountDashboardSearchParams["accountType"],
): value is readonly AccountType[] {
  return Array.isArray(value);
};

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

const accountDashboardSearchParamsToSearchParams = function (
  searchParams: AccountDashboardSearchParams,
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

const pathWithSearchParams = function (
  pathname: string,
  searchParams: URLSearchParams,
): Route {
  const query = searchParams.toString();
  return query === "" ? pathname : `${pathname}?${query}`;
};

const accountWorkspaceSearchParamsToSearchParams = function (
  searchParams: AccountWorkspaceSearchParams,
): URLSearchParams {
  return objectToSearchParams(searchParams);
};

/**
 * App routes related to accounts.
 */
const routes = {
  dashboard: (searchParams: AccountDashboardSearchParams): Route =>
    pathWithSearchParams(
      "/accounts/dashboard",
      accountDashboardSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: AccountWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/accounts/workspace",
      accountWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
