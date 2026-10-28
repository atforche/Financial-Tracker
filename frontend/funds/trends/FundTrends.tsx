import {
  type FundTrendsDataMode,
  type FundTrendsSearchParams,
  getFundTrendsSnapshot,
} from "@/funds/trends/helpers";
import type {
  FundsInAccountingPeriodRange,
  FundsInDateRange,
} from "@/funds/types";
import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import {
  matchesTrendRange,
  resolveTrendRange,
} from "@/framework/routes/trendRange";
import {
  normalizeRequestedFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import { BalanceGranularityModel } from "@/framework/data/api";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import BalanceTrendsChangeChart from "@/framework/charts/BalanceTrendsChangeChart";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import FundBalanceSummaryCards from "@/funds/FundBalanceSummaryCards";
import FundTrendsFilter from "@/funds/trends/FundTrendsFilter";
import FundTrendsListFrame from "@/funds/trends/FundTrendsListFrame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import ReturnLink from "@/framework/view/ReturnLink";
import { buildBalanceTrendChartPoints } from "@/framework/charts/balanceTrendHelpers";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/funds/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the FundTrends component.
 */
interface FundTrendsProps {
  readonly searchParams: Promise<FundTrendsSearchParams>;
}

/**
 * Component that displays the Funds view.
 */
const FundTrends = async function ({
  searchParams,
}: FundTrendsProps): Promise<JSX.Element> {
  const resolvedSearchParams = await searchParams;
  const { returnUrl, sort, page, pageSize, fundName } = resolvedSearchParams;

  const apiClient = await createApiClient();
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Sort: AccountingPeriodSort.DateDescending,
        Limit: 500,
        Offset: 0,
      },
    },
  });
  const accountingPeriods = unwrapApiResponse(
    await accountingPeriodsPromise,
    "Failed to fetch accounting periods",
  );
  const isInOnboardingMode = accountingPeriods.items.length === 0;
  const range = resolveTrendRange(
    resolvedSearchParams,
    accountingPeriods.items,
  );
  const currentMode = range.mode;
  const currentFundNames = normalizeRequestedFundNames(
    toRepeatedSearchParams(fundName),
  );
  const currentPage = normalizePageValue(page);
  const rowsPerPage = getRowsPerPage(pageSize);

  const persistedFilters = {
    ...(typeof sort === "string" ? { sort } : {}),
    ...(shouldPersistFundNames(currentFundNames)
      ? { fundName: currentFundNames }
      : {}),
  };

  if (!matchesTrendRange(resolvedSearchParams, range)) {
    redirect(routes.trends({ returnUrl, ...persistedFilters, ...range }));
  }

  const filterQuery = {
    ...(shouldPersistFundNames(currentFundNames)
      ? { "Filter.Names": [...currentFundNames] }
      : {}),
  };
  const fundQuery = {
    ...(typeof sort === "string" ? { Sort: sort } : {}),
    ...filterQuery,
    Limit: rowsPerPage,
    Offset: getPageOffset(currentPage, rowsPerPage),
  };
  const trends = await (async function (): Promise<
    FundsInDateRange | FundsInAccountingPeriodRange
  > {
    if (range.mode === "date") {
      const rangeQuery = {
        "Range.Start": range.startDate,
        "Range.End": range.endDate,
      };
      return unwrapApiResponse(
        await apiClient.GET("/funds/date-range", {
          params: { query: { ...fundQuery, ...rangeQuery } },
        }),
        "Failed to load fund trends",
      );
    }
    const rangeQuery = {
      "Range.Start": range.startAccountingPeriodId,
      "Range.End": range.endAccountingPeriodId,
    };
    return unwrapApiResponse(
      await apiClient.GET("/funds/accounting-period-range", {
        params: { query: { ...fundQuery, ...rangeQuery } },
      }),
      "Failed to load fund trends",
    );
  })();
  const modeValue: FundTrendsDataMode =
    currentMode === "date" ? "Date" : "AccountingPeriod";
  const periodSummaries =
    "accountingPeriods" in trends ? trends.accountingPeriods : [];
  const dateSummaries = "dates" in trends ? trends.dates : [];
  const openingBalance = "dates" in trends ? trends.openingBalance : null;
  const granularity =
    "dates" in trends ? trends.granularity : BalanceGranularityModel.Day;
  const snapshot = getFundTrendsSnapshot(
    modeValue,
    periodSummaries,
    dateSummaries,
    openingBalance,
    range.mode === "date" ? range.startDate : null,
  );
  const chartPeriods = periodSummaries.map((summary) => ({
    accountingPeriodId: summary.accountingPeriod.id,
    accountingPeriodName: summary.accountingPeriod.name,
    year: summary.accountingPeriod.year,
    month: summary.accountingPeriod.month,
    totalOpeningBalance: summary.openingBalance.totalBalance,
    totalClosingBalance: summary.closingBalance.totalBalance,
  }));
  const balanceTrendChartPoints = buildBalanceTrendChartPoints({
    mode: modeValue,
    accountingPeriods: chartPeriods,
    dates: dateSummaries,
    granularity,
  });
  const selectedFundIds = trends.funds.items
    .filter((fund) =>
      currentFundNames.some(
        (selectedFundName) =>
          selectedFundName.toLocaleLowerCase() ===
          fund.name.toLocaleLowerCase(),
      ),
    )
    .map((fund) => fund.id);
  const hasResolvedSelectedFundIds =
    selectedFundIds.length === currentFundNames.length;
  const currentRange =
    range.mode === "date"
      ? { startDate: range.startDate, endDate: range.endDate }
      : {
          startAccountingPeriodId: range.startAccountingPeriodId,
          endAccountingPeriodId: range.endAccountingPeriodId,
        };
  const transactionWorkspaceHref = transactionRoutes.workspace({
    ...(currentMode === "date"
      ? currentRange
      : {
          accountingPeriodIds: accountingPeriods.items
            .slice(
              Math.min(
                accountingPeriods.items.findIndex(
                  (period) =>
                    period.id === currentRange.startAccountingPeriodId,
                ),
                accountingPeriods.items.findIndex(
                  (period) => period.id === currentRange.endAccountingPeriodId,
                ),
              ),
              Math.max(
                accountingPeriods.items.findIndex(
                  (period) =>
                    period.id === currentRange.startAccountingPeriodId,
                ),
                accountingPeriods.items.findIndex(
                  (period) => period.id === currentRange.endAccountingPeriodId,
                ),
              ) + 1,
            )
            .map((period) => period.id),
        }),
    ...(hasResolvedSelectedFundIds && selectedFundIds.length > 0
      ? { fundIds: selectedFundIds }
      : {}),
    ...(!hasResolvedSelectedFundIds && shouldPersistFundNames(currentFundNames)
      ? { fundNames: currentFundNames }
      : {}),
    returnUrl: routes.trends({
      returnUrl,
      mode: currentMode,
      ...persistedFilters,
      ...currentRange,
    }),
  });

  return (
    <PageLayout>
      <ReturnLink returnUrl={returnUrl} />
      <ConstrainedContent>
        <FundTrendsFilter
          accountingPeriods={accountingPeriods.items}
          availableFundNames={trends.availableFundNames}
        />
      </ConstrainedContent>
      <FundBalanceSummaryCards
        startingLabel={snapshot.startLabel}
        endingLabel={snapshot.endLabel}
        startingBalance={snapshot.startingBalance}
        endingBalance={snapshot.endingBalance}
      />
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
        <BalanceTrendChart
          chartPoints={balanceTrendChartPoints}
          xAxisLabel={modeValue === "Date" ? "Date" : "Accounting Period"}
        />
        <BalanceTrendsChangeChart
          mode={modeValue}
          accountingPeriods={periodSummaries}
          dates={dateSummaries}
          openingBalance={openingBalance?.totalBalance ?? null}
          granularity={granularity}
        />
      </ResponsiveGrid>
      <FundTrendsListFrame
        data={[...trends.funds.items]}
        isInOnboardingMode={isInOnboardingMode}
        totalCount={trends.funds.totalCount}
        transactionWorkspaceHref={transactionWorkspaceHref}
      />
    </PageLayout>
  );
};

export default FundTrends;
