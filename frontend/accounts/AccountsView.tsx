import AccountListFrame from "@/accounts/AccountListFrame";
import type { AccountSortOrder } from "@/accounts/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import breadcrumbs from "@/accounts/breadcrumbs";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Search parameters for the AccountsView component.
 */
interface AccountsViewSearchParams {
  accountSearch?: string;
  accountSort?: AccountSortOrder;
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
  const { accountSearch, accountSort } = await searchParams;

  const apiClient = getApiClient();
  const { data, error } = await apiClient.GET("/accounts", {
    params: {
      query: {
        Search: accountSearch ?? "",
        Sort: accountSort ?? null,
      },
    },
  });
  if (typeof data === "undefined") {
    throw new Error(`Failed to fetch accounts: ${error.detail}`);
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      <SearchBar paramName="search" />
      <AccountListFrame data={data.items} totalCount={data.totalCount} />
    </Stack>
  );
};

export type { AccountsViewSearchParams };
export default AccountsView;
