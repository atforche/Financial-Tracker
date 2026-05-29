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
import type { AccountOverviewDashboardSearchParams } from "@/accounts/overview-dashboard/AccountOverviewDashboard";
import type { AccountType } from "@/accounts/types";
import type { CreateAccountViewSearchParams } from "@/accounts/CreateAccountView";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

const isAccountTypeArray = function (
  value: AccountOverviewDashboardSearchParams["accountType"],
): value is readonly AccountType[] {
  return Array.isArray(value);
};

const accountOverviewDashboardSearchParamsToSearchParams = function (
  searchParams: AccountOverviewDashboardSearchParams,
): URLSearchParams {
  const { accountType, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  if (isAccountTypeArray(accountType)) {
    accountType.forEach((selectedAccountType) => {
      params.append("accountType", selectedAccountType);
    });
  } else if (typeof accountType === "string") {
    params.append("accountType", accountType);
  }

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
  index: (searchParams: AccountOverviewDashboardSearchParams): Route =>
    pathWithSearchParams(
      "/accounts",
      accountOverviewDashboardSearchParamsToSearchParams(searchParams),
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
