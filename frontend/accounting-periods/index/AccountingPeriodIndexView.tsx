import {
  type AccountingPeriodIndexSearchParams,
  buildAccountingPeriodIndexNavigationContext,
} from "@/accounting-periods/index/accountingPeriodIndexNavigationContext";
import AccountingPeriodListFrame from "@/accounting-periods/index/AccountingPeriodListFrame";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Props for the AccountingPeriodIndexView component.
 */
interface AccountingPeriodIndexViewProps {
  readonly searchParams?: Promise<AccountingPeriodIndexSearchParams>;
}

/**
 * Component that displays the Accounting Periods view.
 */
const AccountingPeriodIndexView = async function ({
  searchParams,
}: AccountingPeriodIndexViewProps): Promise<JSX.Element> {
  const searchParamsValue = (await searchParams) ?? null;

  const navigationContext =
    buildAccountingPeriodIndexNavigationContext(searchParamsValue);

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
      <Breadcrumbs breadcrumbs={navigationContext.breadcrumbs} />
      <SearchBar
        paramName={nameof<AccountingPeriodIndexSearchParams>("search")}
      />
      <AccountingPeriodListFrame
        data={accountingPeriods?.items ?? null}
        totalCount={accountingPeriods?.totalCount ?? null}
      />
    </Stack>
  );
};

export default AccountingPeriodIndexView;
