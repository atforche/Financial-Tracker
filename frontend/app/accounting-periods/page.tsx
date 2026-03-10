import { Stack, Typography } from "@mui/material";
import AccountingPeriodListFrame from "@/app/accounting-periods/AccountingPeriodListFrame";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";

/**
 * Props for the AccountingPeriodView component.
 */
interface AccountingPeriodViewProps {
  readonly searchParams?: Promise<{
    query?: string;
    page?: string;
  }>;
}

/**
 * Component that displays the Accounting Period view.
 */
const AccountingPeriodView = async function({
  searchParams: searchParamsPromise,
}: AccountingPeriodViewProps): Promise<JSX.Element> {
  const searchParams = await searchParamsPromise;
  return (
    <Stack spacing={2}>
      <Typography variant="h6">
        Accounting Periods
      </Typography>
      <SearchBar paramName="query" />
      <AccountingPeriodListFrame queryString={searchParams?.query ?? ""} />
    </Stack>
  );
};

export default AccountingPeriodView;
