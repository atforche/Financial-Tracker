import {
  AccountingPeriodSort,
  type AccountingPeriodWithBalanceSort,
} from "@/accounting-periods/types";
import type { Transaction, TransactionSort } from "@/transactions/types";
import {
  getPageOffset,
  normalizePageValue,
  rowsPerPage,
} from "@/framework/listframe/page";
import AccountingPeriodTrendChart from "@/accounting-periods/trends/AccountingPeriodTrendChart";
import AccountingPeriodTrendsChangeChart from "@/accounting-periods/trends/AccountingPeriodTrendsChangeChart";
import AccountingPeriodTrendsFilter from "@/accounting-periods/trends/AccountingPeriodTrendsFilter";
import AccountingPeriodTrendsListFrame from "@/accounting-periods/trends/AccountingPeriodTrendsListFrame";
import AccountingPeriodTrendsSummaryCards from "@/accounting-periods/trends/AccountingPeriodTrendsSummaryCards";
import AccountingPeriodTrendsTransactionListFrame from "@/accounting-periods/trends/AccountingPeriodTrendsTransactionListFrame";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import IncomeSpendingCard from "@/transactions/IncomeSpendingCard";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import createApiClient from "@/framework/data/createApiClient";
import { createEmptyTrends } from "@/accounting-periods/trends/helpers";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters for the AccountingPeriodTrends component.
 */
interface AccountingPeriodTrendsSearchParams {
  sort?: AccountingPeriodWithBalanceSort;
  page?: number | string | null;
  transactionSort?: TransactionSort;
  transactionPage?: number | string | null;
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
}

/**
 * Props for the AccountingPeriodTrends component.
 */
interface AccountingPeriodTrendsProps {
  readonly searchParams: Promise<AccountingPeriodTrendsSearchParams>;
}

/**
 * Component that displays the Accounting Period trends.
 */
const AccountingPeriodTrends = async function ({
  searchParams,
}: AccountingPeriodTrendsProps): Promise<JSX.Element> {
  const {
    sort,
    page,
    transactionSort,
    transactionPage,
    startAccountingPeriodId,
    endAccountingPeriodId,
  } = await searchParams;

  const apiClient = await createApiClient();
  const accountingPeriodsResponse = await apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Sort: AccountingPeriodSort.DateDescending,
        Limit: 500,
        Offset: 0,
      },
    },
  });
  const accountingPeriods = unwrapApiResponse(
    accountingPeriodsResponse,
    "Failed to fetch accounting periods",
  );
  const currentPage = normalizePageValue(page);
  const currentTransactionPage = normalizePageValue(transactionPage);

  const latestAccountingPeriod = accountingPeriods.items[0] ?? null;
  let trends = createEmptyTrends();
  let transactions: { items: Transaction[]; totalCount: number } = {
    items: [],
    totalCount: 0,
  };
  if (latestAccountingPeriod !== null) {
    const range = {
      "Range.Start": startAccountingPeriodId ?? latestAccountingPeriod.id,
      "Range.End": endAccountingPeriodId ?? latestAccountingPeriod.id,
    };
    const [trendsResponse, transactionResponse] = await Promise.all([
      apiClient.GET("/accounting-periods/range", {
        params: {
          query: {
            ...range,
            ...(isNotNullOrUndefined(sort) ? { Sort: sort } : {}),
            Limit: rowsPerPage,
            Offset: getPageOffset(currentPage),
          },
        },
      }),
      apiClient.GET("/transactions/accounting-period-range", {
        params: {
          query: {
            ...range,
            ...(isNotNullOrUndefined(transactionSort)
              ? { Sort: transactionSort }
              : {}),
            Limit: rowsPerPage,
            Offset: getPageOffset(currentTransactionPage),
          },
        },
      }),
    ]);
    trends = unwrapApiResponse(
      trendsResponse,
      "Failed to fetch accounting period trends",
    );
    const { transactions: responseTransactions } = unwrapApiResponse(
      transactionResponse,
      "Failed to fetch transactions for the accounting period range",
    );
    transactions = responseTransactions;
  }

  return (
    <PageLayout>
      <ConstrainedContent>
        <AccountingPeriodTrendsFilter
          accountingPeriods={accountingPeriods.items}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          disabled={latestAccountingPeriod === null}
        />
      </ConstrainedContent>
      <AccountingPeriodTrendsSummaryCards
        accountingPeriods={trends.accountingPeriods.items}
      />
      <IncomeSpendingCard
        totalIncome={trends.totalIncome}
        totalSpending={trends.totalSpending}
      />
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
        <AccountingPeriodTrendChart
          accountingPeriods={trends.accountingPeriods.items}
        />
        <AccountingPeriodTrendsChangeChart
          accountingPeriods={trends.accountingPeriods.items}
        />
      </ResponsiveGrid>
      <ResponsiveGrid minimumColumnWidth={800}>
        <AccountingPeriodTrendsListFrame
          data={trends.accountingPeriods.items}
          totalCount={trends.accountingPeriods.totalCount}
        />
        <AccountingPeriodTrendsTransactionListFrame
          transactions={transactions.items}
          totalCount={transactions.totalCount}
        />
      </ResponsiveGrid>
    </PageLayout>
  );
};

export type { AccountingPeriodTrendsSearchParams };
export default AccountingPeriodTrends;
