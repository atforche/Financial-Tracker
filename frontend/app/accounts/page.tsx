import AccountListFrame from "@/app/accounts/AccountListFrame";
import type { AccountSortOrder } from "@/data/accountTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import getApiClient from "@/data/getApiClient";
import routes from "@/framework/routes";

/**
 * Props for the Page component.
 */
interface PageProps {
  readonly searchParams: Promise<{
    accountSearch?: string;
    accountSort?: AccountSortOrder;
  }>;
}

/**
 * Component that displays the Accounts view.
 */
const Page = async function ({
  searchParams,
}: PageProps): Promise<JSX.Element> {
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
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounts",
            href: routes.accounts.index,
          },
        ]}
      />
      <SearchBar paramName="search" />
      <AccountListFrame data={data.items} totalCount={data.totalCount} />
    </Stack>
  );
};

export default Page;
