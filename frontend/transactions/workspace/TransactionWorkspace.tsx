import type { TransactionSort, TransactionType } from "@/transactions/types";
import type { AccountType } from "@/accounts/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ReturnLink from "@/framework/view/ReturnLink";
import TransactionWorkspaceFilter from "@/transactions/workspace/TransactionWorkspaceFilter";
import TransactionWorkspaceListFrame from "@/transactions/workspace/TransactionWorkspaceListFrame";
import { Typography } from "@mui/material";
import { getDefaultTrendDateRange } from "@/framework/routes/trendRange";
import { getTransactionWorkspaceListData } from "@/transactions/workspace/getTransactionWorkspaceData";
import { normalizeFullyPosted } from "@/transactions/fullyPostedFilter";
import { redirect } from "next/navigation";
import routes from "@/transactions/routes";

/**
 * Search parameters supported by the Transactions workspace.
 */
interface TransactionWorkspaceSearchParams {
  accountingPeriodIds?: string | readonly string[];
  accountIds?: string | readonly string[];
  fundIds?: string | readonly string[];
  locationIds?: string | readonly string[];
  fundNames?: string | readonly string[];
  accountTypes?: AccountType | readonly AccountType[];
  accountNames?: string | readonly string[];
  transactionTypes?: TransactionType | readonly TransactionType[];
  fullyPosted?: string | readonly string[];
  startDate?: string;
  endDate?: string;
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  sort?: TransactionSort | null;
  page?: number | string | null;
  selectedTransactionId?: string;
  returnUrl?: string;
}

/**
 * Props for the TransactionWorkspace component.
 */
interface TransactionWorkspaceProps {
  readonly searchParams: Promise<TransactionWorkspaceSearchParams>;
}

/**
 * Renders the main transaction workspace, including filters and the transaction list.
 */
const TransactionWorkspace = async function ({
  searchParams,
}: TransactionWorkspaceProps): Promise<JSX.Element> {
  const resolvedSearchParams = await searchParams;
  // Filters other than the date range narrow the search
  const hasFilter =
    [
      resolvedSearchParams.accountingPeriodIds,
      resolvedSearchParams.accountIds,
      resolvedSearchParams.fundIds,
      resolvedSearchParams.locationIds,
      resolvedSearchParams.fundNames,
      resolvedSearchParams.accountTypes,
      resolvedSearchParams.accountNames,
      resolvedSearchParams.transactionTypes,
    ].some((value) =>
      Array.isArray(value) ? value.length > 0 : typeof value !== "undefined",
    ) || normalizeFullyPosted(resolvedSearchParams.fullyPosted) !== null;
  // With nothing to narrow the list, start on the recent range the other range pages use, as some sorts load the whole range
  // A link with a filter, such as unposted transactions, keeps every transaction it matches
  if (
    !hasFilter &&
    [
      resolvedSearchParams.accountingPeriodIds,
      resolvedSearchParams.startDate,
      resolvedSearchParams.endDate,
      resolvedSearchParams.startAccountingPeriodId,
      resolvedSearchParams.endAccountingPeriodId,
    ].every((value) => typeof value === "undefined")
  ) {
    const defaultRange = getDefaultTrendDateRange();
    redirect(
      routes.workspace({
        ...resolvedSearchParams,
        startDate: defaultRange.start,
        endDate: defaultRange.end,
      }),
    );
  }
  const {
    accountingPeriods,
    accounts,
    funds,
    locations,
    selectedAccountIds,
    selectedFundIds,
    transactions,
  } = await getTransactionWorkspaceListData(resolvedSearchParams);

  return (
    <ConstrainedContent>
      <PageLayout>
        <ReturnLink returnUrl={resolvedSearchParams.returnUrl} />
        <Typography variant="h4">Transactions</Typography>
        <TransactionWorkspaceFilter
          accountingPeriods={accountingPeriods}
          accounts={accounts}
          funds={funds}
          locations={locations}
          selectedAccountIds={selectedAccountIds}
          selectedFundIds={selectedFundIds}
        />
        <TransactionWorkspaceListFrame
          data={transactions.items}
          totalCount={transactions.totalCount}
          hasActiveFilters={hasFilter}
        />
      </PageLayout>
    </ConstrainedContent>
  );
};

export type { TransactionWorkspaceSearchParams };
export default TransactionWorkspace;
