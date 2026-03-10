import { Stack, Typography } from "@mui/material";
import AccountingPeriodListFrame from "@/app/accounting-periods/AccountingPeriodListFrame";
import type { AccountingPeriodSortOrder } from "@/data/accountingPeriodTypes";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the AccountingPeriodView component.
 */
interface AccountingPeriodViewProps {
  readonly searchParams?: Promise<{
    query?: string;
    sort?: AccountingPeriodSortOrder;
    page?: string;
  }>;
}

/**
 * Component that displays the Accounting Period view.
 */
const AccountingPeriodView = async function ({
  searchParams: searchParamsPromise,
}: AccountingPeriodViewProps): Promise<JSX.Element> {
  const searchParams = await searchParamsPromise;
  const client = getApiClient();
  const { data } = await client.GET("/accounting-periods", {
    params: {
      query: {
        QueryString: searchParams?.query ?? "",
        SortBy: searchParams?.sort ?? null,
      },
    },
  });

  return (
    <Stack spacing={2}>
      <Typography variant="h6">Accounting Periods</Typography>
      <SearchBar paramName="query" />
      <AccountingPeriodListFrame data={data?.items ?? null} />
    </Stack>
  );
};

export default AccountingPeriodView;
