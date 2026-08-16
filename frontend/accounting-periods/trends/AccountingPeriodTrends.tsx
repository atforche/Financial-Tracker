import {
  AccountingPeriodSort,
  type AccountingPeriodWithBalanceSort,
} from "@/accounting-periods/types";
import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import AccountingPeriodTrendChart from "@/accounting-periods/trends/AccountingPeriodTrendChart";
import AccountingPeriodTrendsChangeChart from "@/accounting-periods/trends/AccountingPeriodTrendsChangeChart";
import AccountingPeriodTrendsFilter from "@/accounting-periods/trends/AccountingPeriodTrendsFilter";
import AccountingPeriodTrendsListFrame from "@/accounting-periods/trends/AccountingPeriodTrendsListFrame";
import AccountingPeriodTrendsSummaryCards from "@/accounting-periods/trends/AccountingPeriodTrendsSummaryCards";
import ActualIncomeCard from "@/transactions/ActualIncomeCard";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import ExpectedGoalContributionsActualCard from "@/accounting-periods/workspace/ExpectedGoalContributionsActualCard";
import ExpectedIncomeActualCard from "@/accounting-periods/workspace/ExpectedIncomeActualCard";
import type { IncomeAmount } from "@/transactions/types";
import IncomeSpendingCard from "@/transactions/IncomeSpendingCard";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import ResponsivePageSize from "@/framework/listframe/ResponsivePageSize";
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
  pageSize?: number | string | null;
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
    pageSize,
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
  const rowsPerPage = getRowsPerPage(pageSize);

  const latestAccountingPeriod = accountingPeriods.items[0] ?? null;
  let trends = createEmptyTrends();
  if (latestAccountingPeriod !== null) {
    const range = {
      "Range.Start": startAccountingPeriodId ?? latestAccountingPeriod.id,
      "Range.End": endAccountingPeriodId ?? latestAccountingPeriod.id,
    };
    const trendsResponse = await apiClient.GET("/accounting-periods/range", {
      params: {
        query: {
          ...range,
          ...(isNotNullOrUndefined(sort) ? { Sort: sort } : {}),
          Limit: rowsPerPage,
          Offset: getPageOffset(currentPage, rowsPerPage),
        },
      },
    });
    trends = unwrapApiResponse(
      trendsResponse,
      "Failed to fetch accounting period trends",
    );
  }
  const rangeExpectations = trends.accountingPeriods.items.reduce<{
    expectedIncome: IncomeAmount;
    actualIncome: IncomeAmount;
    expectedGoalContributions: number;
    actualGoalContributions: number;
  }>(
    (totals, accountingPeriod) => ({
      expectedIncome: {
        total:
          totals.expectedIncome.total + accountingPeriod.expectedIncome.total,
        tracked:
          totals.expectedIncome.tracked +
          accountingPeriod.expectedIncome.tracked,
        untracked:
          totals.expectedIncome.untracked +
          accountingPeriod.expectedIncome.untracked,
      },
      actualIncome: {
        total: totals.actualIncome.total + accountingPeriod.actualIncome.total,
        tracked:
          totals.actualIncome.tracked + accountingPeriod.actualIncome.tracked,
        untracked:
          totals.actualIncome.untracked +
          accountingPeriod.actualIncome.untracked,
      },
      expectedGoalContributions:
        totals.expectedGoalContributions +
        accountingPeriod.expectedGoalContributions,
      actualGoalContributions:
        totals.actualGoalContributions +
        accountingPeriod.actualGoalContributions,
    }),
    {
      expectedIncome: { total: 0, tracked: 0, untracked: 0 },
      actualIncome: { total: 0, tracked: 0, untracked: 0 },
      expectedGoalContributions: 0,
      actualGoalContributions: 0,
    },
  );

  return (
    <PageLayout>
      <ResponsivePageSize desktopBreakpoint="lg" />
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
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }} spacing={2}>
        <ActualIncomeCard totalIncome={trends.totalIncome} />
        <IncomeSpendingCard
          totalIncome={trends.totalIncome}
          totalSpending={trends.totalSpending}
        />
        <ExpectedIncomeActualCard
          expectedIncome={rangeExpectations.expectedIncome}
          actualIncome={rangeExpectations.actualIncome}
        />
        <ExpectedGoalContributionsActualCard
          expectedGoalContributions={
            rangeExpectations.expectedGoalContributions
          }
          actualGoalContributions={rangeExpectations.actualGoalContributions}
        />
      </ResponsiveGrid>
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
      </ResponsiveGrid>
    </PageLayout>
  );
};

export type { AccountingPeriodTrendsSearchParams };
export default AccountingPeriodTrends;
