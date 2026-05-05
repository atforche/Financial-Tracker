import AccountListFrame from "@/accounts/index/AccountListFrame";
import type { AccountSortOrder } from "@/accounts/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import breadcrumbs from "@/accounts/breadcrumbs";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";

/**
 * Search parameters for the AccountIndexView component.
 */
interface AccountIndexViewSearchParams {
  search?: string;
  sort?: AccountSortOrder;
  page?: number;
}

/**
 * Props for the AccountIndexView component.
 */
interface AccountIndexViewProps {
  readonly searchParams: Promise<AccountIndexViewSearchParams>;
}

/**
 * Component that displays the Account Index view.
 */
const AccountIndexView = async function ({
  searchParams,
}: AccountIndexViewProps): Promise<JSX.Element> {
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
      <SearchBar paramName={nameof<AccountIndexViewSearchParams>("search")} />
      <AccountListFrame
        data={accounts.items}
        totalCount={accounts.totalCount}
      />
    </Stack>
  );
};

export type { AccountIndexViewSearchParams };
export default AccountIndexView;
