import AccountingPeriodListFrame from "@/app/accounting-periods/AccountingPeriodListFrame";
import type { AccountingPeriodSortOrder } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import getApiClient from "@/data/getApiClient";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Props for the AccountingPeriodView component.
 */
interface AccountingPeriodViewProps {
  readonly searchParams?: Promise<{
    query?: string;
    sort?: AccountingPeriodSortOrder;
    page?: number;
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
        Limit: rowsPerPage,
        Offset: ((searchParams?.page ?? 1) - 1) * rowsPerPage,
      },
    },
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={[
          { label: "Accounting Periods", href: "/accounting-periods" },
        ]}
      />
      <SearchBar paramName="query" />
      <AccountingPeriodListFrame
        data={data?.items ?? null}
        totalCount={data?.totalCount ?? null}
      />
    </Stack>
  );
};

export default AccountingPeriodView;
