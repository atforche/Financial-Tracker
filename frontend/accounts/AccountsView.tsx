import AccountListFrame from "@/accounts/AccountListFrame";
import type { AccountSortOrder } from "@/accounts/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import breadcrumbs from "@/accounts/breadcrumbs";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the AccountsView component.
 */
interface AccountsViewSearchParams {
  search?: string;
  sort?: AccountSortOrder;
  page?: number;
}

/**
 * Props for the AccountsView component.
 */
interface AccountsViewProps {
  readonly searchParams: Promise<AccountsViewSearchParams>;
}

/**
 * Component that displays the Accounts view.
 */
const AccountsView = async function ({
  searchParams,
}: AccountsViewProps): Promise<JSX.Element> {
  const { search, sort, page } = await searchParams;

  const apiClient = getApiClient();
  const accountsPromise = apiClient.GET("/accounts", {
    params: {
      query: {
        Search: search ?? "",
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: ((page ?? 1) - 1) * rowsPerPage,
      },
    },
  });
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: null,
        Limit: 1,
        Offset: 0,
      },
    },
  });

  const [{ data: accounts }, { data: accountingPeriods }] = await Promise.all([
    accountsPromise,
    accountingPeriodsPromise,
  ]);
  if (
    typeof accounts === "undefined" ||
    typeof accountingPeriods === "undefined"
  ) {
    throw new Error(`Failed to fetch accounts`);
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      <SearchBar paramName={nameof<AccountsViewSearchParams>("search")} />
      <AccountListFrame
        data={accounts.items}
        isInOnboardingMode={accountingPeriods.totalCount === 0}
        totalCount={accounts.totalCount}
      />
    </Stack>
  );
};

export type { AccountsViewSearchParams };
export default AccountsView;
