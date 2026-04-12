import AccountingPeriodListFrame from "@/app/accounting-periods/AccountingPeriodListFrame";
import type { AccountingPeriodSortOrder } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import getApiClient from "@/data/getApiClient";
import routes from "@/framework/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Props for the AccountingPeriodView component.
 */
interface AccountingPeriodViewProps {
  readonly searchParams?: Promise<{
    search?: string;
    sort?: AccountingPeriodSortOrder;
    page?: number;
  }>;
}

/**
 * Component that displays the Accounting Period view.
 */
const AccountingPeriodView = async function ({
  searchParams,
}: AccountingPeriodViewProps): Promise<JSX.Element> {
  const searchParamsValue = await searchParams;
  const client = getApiClient();
  const { data } = await client.GET("/accounting-periods", {
    params: {
      query: {
        Search: searchParamsValue?.search ?? "",
        Sort: searchParamsValue?.sort ?? null,
        Limit: rowsPerPage,
        Offset: ((searchParamsValue?.page ?? 1) - 1) * rowsPerPage,
      },
    },
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={[
          { label: "Accounting Periods", href: routes.accountingPeriods.index },
        ]}
      />
      <SearchBar paramName="search" />
      <AccountingPeriodListFrame
        data={data?.items ?? null}
        totalCount={data?.totalCount ?? null}
      />
    </Stack>
  );
};

export default AccountingPeriodView;
