import AccountingPeriodListFrame from "@/accounting-periods/index/AccountingPeriodListFrame";
import type { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import breadcrumbs from "@/accounting-periods/breadcrumbs";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the AccountingPeriodIndexView component.
 */
interface AccountingPeriodIndexViewSearchParams {
  search?: string;
  sort?: AccountingPeriodSortOrder;
  page?: number;
}

/**
 * Props for the AccountingPeriodIndexView component.
 */
interface AccountingPeriodIndexViewProps {
  readonly searchParams?: Promise<AccountingPeriodIndexViewSearchParams>;
}

/**
 * Component that displays the Accounting Periods view.
 */
const AccountingPeriodIndexView = async function ({
  searchParams,
}: AccountingPeriodIndexViewProps): Promise<JSX.Element> {
  const searchParamsValue = await searchParams;
  const client = getApiClient();
  const { data: accountingPeriods } = await client.GET("/accounting-periods", {
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
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      <SearchBar
        paramName={nameof<AccountingPeriodIndexViewSearchParams>("search")}
      />
      <AccountingPeriodListFrame
        data={accountingPeriods?.items ?? null}
        totalCount={accountingPeriods?.totalCount ?? null}
      />
    </Stack>
  );
};

export type { AccountingPeriodIndexViewSearchParams };
export default AccountingPeriodIndexView;
