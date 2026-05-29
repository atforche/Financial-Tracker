import type {
  AccountViewParams,
  AccountViewSearchParams,
} from "@/accounts/AccountView";
import type {
  DeleteAccountViewParams,
  DeleteAccountViewSearchParams,
} from "@/accounts/DeleteAccountView";
import type {
  UpdateAccountViewParams,
  UpdateAccountViewSearchParams,
} from "@/accounts/UpdateAccountView";
import type { AccountDashboardSearchParams } from "@/accounts/dashboard/AccountDashboard";
import type { AccountType } from "@/accounts/types";
import type { CreateAccountViewSearchParams } from "@/accounts/CreateAccountView";
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

/**
 * App routes related to accounts.
 */
const routes = {
  index: (searchParams: AccountDashboardSearchParams): Route =>
    pathWithSearchParams(
      "/accounts",
      accountDashboardSearchParamsToSearchParams(searchParams),
    ),
  create: (searchParams: CreateAccountViewSearchParams): Route =>
    pathWithSearchParams(
      "/accounts/create",
      objectToSearchParams(searchParams),
    ),
  onboard: "/accounts/onboard" as Route,
  detail: (
    params: AccountViewParams,
    searchParams: AccountViewSearchParams,
  ): Route =>
    pathWithSearchParams(
      `/accounts/${params.id}`,
      objectToSearchParams(searchParams),
    ),
  update: (
    params: UpdateAccountViewParams,
    searchParams: UpdateAccountViewSearchParams,
  ): Route =>
    pathWithSearchParams(
      `/accounts/${params.id}/update`,
      objectToSearchParams(searchParams),
    ),
  delete: (
    params: DeleteAccountViewParams,
    searchParams: DeleteAccountViewSearchParams,
  ): Route =>
    pathWithSearchParams(
      `/accounts/${params.id}/delete`,
      objectToSearchParams(searchParams),
    ),
};

export default routes;
