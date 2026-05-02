import AccountListFrame from "@/accounts/AccountListFrame";
import type { AccountSortOrder } from "@/accounts/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import breadcrumbs from "@/accounts/breadcrumbs";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";

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
  const { search, sort } = await searchParams;

  const apiClient = getApiClient();
  const { data: accounts, error } = await apiClient.GET("/accounts", {
    params: {
      query: {
        Search: search ?? "",
        Sort: sort ?? null,
      },
    },
  });
  if (typeof accounts === "undefined") {
    throw new Error(`Failed to fetch accounts: ${error.detail}`);
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      <SearchBar paramName={nameof<AccountsViewSearchParams>("search")} />
      <AccountListFrame
        data={accounts.items}
        totalCount={accounts.totalCount}
      />
    </Stack>
  );
};

export type { AccountsViewSearchParams };
export default AccountsView;
