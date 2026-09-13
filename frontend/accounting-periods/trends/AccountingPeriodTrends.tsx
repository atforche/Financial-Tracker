import {
  AccountingPeriodSort,
  type AccountingPeriodWithBalanceSort,
} from "@/accounting-periods/types";
import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import AccountBalanceSummaryCards from "@/accounts/AccountBalanceSummaryCards";
import AccountingPeriodProgressFlow from "@/accounting-periods/workspace/AccountingPeriodProgressFlow";
import AccountingPeriodTrendChart from "@/accounting-periods/trends/AccountingPeriodTrendChart";
import AccountingPeriodTrendsChangeChart from "@/accounting-periods/trends/AccountingPeriodTrendsChangeChart";
import AccountingPeriodTrendsFilter from "@/accounting-periods/trends/AccountingPeriodTrendsFilter";
import AccountingPeriodTrendsListFrame from "@/accounting-periods/trends/AccountingPeriodTrendsListFrame";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import FundBalanceSummaryCards from "@/funds/FundBalanceSummaryCards";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import { compareAccountingPeriods } from "@/accounting-periods/helpers";
import createApiClient from "@/framework/data/createApiClient";
import { createEmptyTrends } from "@/accounting-periods/trends/helpers";
import { getAccountTrendsSnapshot } from "@/accounts/trends/helpers";
import { getDefaultTrendAccountingPeriodRange } from "@/framework/routes/trendRange";
import { getFundTrendsSnapshot } from "@/funds/trends/helpers";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";
import routes from "@/accounting-periods/routes";
import transactionRoutes from "@/transactions/routes";
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
  const defaultAccountingPeriodRange = getDefaultTrendAccountingPeriodRange(
    accountingPeriods.items,
  );
  const defaultStartAccountingPeriodId =
    defaultAccountingPeriodRange?.start ?? null;
  const defaultEndAccountingPeriodId =
    defaultAccountingPeriodRange?.end ?? null;
  const selectedAccountingPeriodIds = ((): string[] => {
    if (latestAccountingPeriod === null) {
      return [];
    }
    const startId =
      startAccountingPeriodId ?? defaultStartAccountingPeriodId ?? "";
    const endId = endAccountingPeriodId ?? defaultEndAccountingPeriodId ?? "";
    const startIndex = accountingPeriods.items.findIndex(
      (period) => period.id === startId,
    );
    const endIndex = accountingPeriods.items.findIndex(
      (period) => period.id === endId,
    );
    return startIndex < 0 || endIndex < 0
      ? []
      : accountingPeriods.items
          .slice(
            Math.min(startIndex, endIndex),
            Math.max(startIndex, endIndex) + 1,
          )
          .map((period) => period.id);
  })();
  let trends = createEmptyTrends();
  let accountBalanceSnapshot = getAccountTrendsSnapshot(
    "AccountingPeriod",
    [],
    [],
  );
  let fundBalanceSnapshot = getFundTrendsSnapshot("AccountingPeriod", [], []);
  let rangeExpectations = {
    expectedGoalContributions: 0,
    actualGoalContributions: 0,
    actualExtraGoalContributions: 0,
  };
  if (latestAccountingPeriod !== null) {
    const range = {
      "Range.Start":
        startAccountingPeriodId ?? defaultStartAccountingPeriodId ?? "",
      "Range.End": endAccountingPeriodId ?? defaultEndAccountingPeriodId ?? "",
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
    const [accountTrendsResponse, fundTrendsResponse, rangePeriodsResponse] =
      await Promise.all([
        apiClient.GET("/accounts/accounting-period-range", {
          params: { query: { ...range, Limit: 1, Offset: 0 } },
        }),
        apiClient.GET("/funds/accounting-period-range", {
          params: { query: { ...range, Limit: 1, Offset: 0 } },
        }),
        apiClient.GET("/accounting-periods/range", {
          params: {
            query: {
              ...range,
              Limit: trends.accountingPeriods.totalCount,
              Offset: 0,
            },
          },
        }),
      ]);
    accountBalanceSnapshot = getAccountTrendsSnapshot(
      "AccountingPeriod",
      unwrapApiResponse(
        accountTrendsResponse,
        "Failed to fetch account balance trends",
      ).accountingPeriods,
      [],
    );
    fundBalanceSnapshot = getFundTrendsSnapshot(
      "AccountingPeriod",
      unwrapApiResponse(
        fundTrendsResponse,
        "Failed to fetch fund balance trends",
      ).accountingPeriods,
      [],
    );
    rangeExpectations = unwrapApiResponse(
      rangePeriodsResponse,
      "Failed to fetch accounting period range totals",
    ).accountingPeriods.items.reduce(
      (totals, accountingPeriod) => ({
        expectedGoalContributions:
          totals.expectedGoalContributions +
          accountingPeriod.expectedGoalContributions,
        actualGoalContributions:
          totals.actualGoalContributions +
          accountingPeriod.actualGoalContributions,
        actualExtraGoalContributions:
          totals.actualExtraGoalContributions +
          accountingPeriod.actualExtraGoalContributions,
      }),
      rangeExpectations,
    );
  }
  const chronologicalAccountingPeriods = [
    ...trends.accountingPeriods.items,
  ].sort(compareAccountingPeriods);
  const transactionWorkspaceHref =
    selectedAccountingPeriodIds.length === 0
      ? null
      : transactionRoutes.workspace({
          accountingPeriodIds: selectedAccountingPeriodIds,
          returnUrl: routes.trends({
            ...(typeof sort === "undefined" ? {} : { sort }),
            ...(typeof page === "undefined" ? {} : { page }),
            ...(typeof pageSize === "undefined" ? {} : { pageSize }),
            ...(latestAccountingPeriod === null
              ? {}
              : {
                  startAccountingPeriodId:
                    startAccountingPeriodId ??
                    defaultStartAccountingPeriodId ??
                    "",
                  endAccountingPeriodId:
                    endAccountingPeriodId ?? defaultEndAccountingPeriodId ?? "",
                }),
          }),
        });

  return (
    <PageLayout>
      <ConstrainedContent>
        <AccountingPeriodTrendsFilter
          accountingPeriods={accountingPeriods.items}
          disabled={latestAccountingPeriod === null}
        />
      </ConstrainedContent>
      <AccountBalanceSummaryCards
        startingLabel={accountBalanceSnapshot.startLabel}
        endingLabel={accountBalanceSnapshot.endLabel}
        startingBalance={accountBalanceSnapshot.startingBalance}
        endingBalance={accountBalanceSnapshot.endingBalance}
        titlePrefix="Account"
      />
      <FundBalanceSummaryCards
        startingLabel={fundBalanceSnapshot.startLabel}
        endingLabel={fundBalanceSnapshot.endLabel}
        startingBalance={fundBalanceSnapshot.startingBalance}
        endingBalance={fundBalanceSnapshot.endingBalance}
        titlePrefix="Fund"
      />
      <AccountingPeriodProgressFlow
        expectedIncome={trends.totalExpectedIncome}
        actualIncome={trends.totalIncome}
        totalSpending={trends.totalSpending}
        expectedFundGoalContributions={
          rangeExpectations.expectedGoalContributions
        }
        actualFundGoalContributions={rangeExpectations.actualGoalContributions}
        actualExtraFundGoalContributions={
          rangeExpectations.actualExtraGoalContributions
        }
      />
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
        <AccountingPeriodTrendChart
          accountingPeriods={chronologicalAccountingPeriods}
        />
        <AccountingPeriodTrendsChangeChart
          accountingPeriods={chronologicalAccountingPeriods}
        />
      </ResponsiveGrid>
      <ResponsiveGrid minimumColumnWidth={800}>
        <AccountingPeriodTrendsListFrame
          data={trends.accountingPeriods.items}
          totalCount={trends.accountingPeriods.totalCount}
          transactionWorkspaceHref={transactionWorkspaceHref}
        />
      </ResponsiveGrid>
    </PageLayout>
  );
};

export type { AccountingPeriodTrendsSearchParams };
export default AccountingPeriodTrends;
