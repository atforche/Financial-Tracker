import type {
  FundBalanceEvent,
  FundsInAccountingPeriodRange,
  FundsInDateRange,
} from "@/funds/types";
import type {
  FundTrendsDataMode,
  FundTrendsSearchParams,
} from "@/funds/trends/helpers";
import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import {
  normalizeRequestedFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import FundTrendsAssignmentSpendingCard from "@/funds/trends/FundTrendsAssignmentSpendingCard";
import FundTrendsBalanceEventListFrame from "@/funds/trends/FundTrendsBalanceEventListFrame";
import FundTrendsChangeChart from "@/funds/trends/FundTrendsChangeChart";
import FundTrendsFilter from "@/funds/trends/FundTrendsFilter";
import FundTrendsListFrame from "@/funds/trends/FundTrendsListFrame";
import FundTrendsSummaryCards from "@/funds/trends/FundTrendsSummaryCards";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import { buildBalanceTrendChartPoints } from "@/framework/charts/balanceTrendHelpers";
import createApiClient from "@/framework/data/createApiClient";
import dayjs from "dayjs";
import { redirect } from "next/navigation";
import routes from "@/funds/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
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
  const {
    sort,
    page,
    pageSize,
    balanceEventSort,
    balanceEventPage,
    mode,
    fundName,
    startAccountingPeriodId,
    endAccountingPeriodId,
    startDate,
    endDate,
  } = await searchParams;

  const defaultEndDate = dayjs();
  const defaultStartDate = defaultEndDate.subtract(90, "day");

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
  const latestAccountingPeriod = accountingPeriods.items[0] ?? null;
  const isInOnboardingMode = latestAccountingPeriod === null;
  const currentMode =
    typeof mode === "undefined" || isInOnboardingMode ? "date" : mode;
  const currentFundNames = normalizeRequestedFundNames(
    toRepeatedSearchParams(fundName),
  );
  const currentPage = normalizePageValue(page);
  const rowsPerPage = getRowsPerPage(pageSize);
  const currentBalanceEventPage = normalizePageValue(balanceEventPage);

  const persistedFilters = {
    ...(typeof sort === "string" ? { sort } : {}),
    ...(typeof balanceEventSort === "string" ? { balanceEventSort } : {}),
    ...(shouldPersistFundNames(currentFundNames)
      ? { fundName: currentFundNames }
      : {}),
  };

  if (
    (currentMode === "date" &&
      (typeof startDate === "undefined" || typeof endDate === "undefined")) ||
    (currentMode === "accounting-period" && latestAccountingPeriod === null)
  ) {
    redirect(
      routes.trends({
        mode: "date",
        ...persistedFilters,
        startDate: defaultStartDate.format("YYYY-MM-DD"),
        endDate: defaultEndDate.format("YYYY-MM-DD"),
      }),
    );
  }

  if (
    currentMode === "accounting-period" &&
    latestAccountingPeriod !== null &&
    (typeof startAccountingPeriodId === "undefined" ||
      typeof endAccountingPeriodId === "undefined")
  ) {
    redirect(
      routes.trends({
        mode: "accounting-period",
        ...persistedFilters,
        startAccountingPeriodId: latestAccountingPeriod.id,
        endAccountingPeriodId: latestAccountingPeriod.id,
      }),
    );
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
  const balanceEventQuery = {
    ...(typeof balanceEventSort === "string" ? { Sort: balanceEventSort } : {}),
    ...filterQuery,
    Limit: rowsPerPage,
    Offset: getPageOffset(currentBalanceEventPage, rowsPerPage),
  };
  const { trends, balanceEvents } = await (async function (): Promise<{
    trends: FundsInDateRange | FundsInAccountingPeriodRange;
    balanceEvents: { items: FundBalanceEvent[]; totalCount: number };
  }> {
    if (currentMode === "date") {
      const range = {
        "Range.Start": startDate ?? defaultStartDate.format("YYYY-MM-DD"),
        "Range.End": endDate ?? defaultEndDate.format("YYYY-MM-DD"),
      };
      const [fundResponse, balanceEventResponse] = await Promise.all([
        apiClient.GET("/funds/date-range", {
          params: { query: { ...fundQuery, ...range } },
        }),
        apiClient.GET("/funds/balance-events/date-range", {
          params: { query: { ...balanceEventQuery, ...range } },
        }),
      ]);
      return {
        trends: unwrapApiResponse(fundResponse, "Failed to load fund trends"),
        balanceEvents: unwrapApiResponse(
          balanceEventResponse,
          "Failed to load fund balance events",
        ),
      };
    }
    const range = {
      "Range.Start":
        startAccountingPeriodId ?? latestAccountingPeriod?.id ?? "",
      "Range.End": endAccountingPeriodId ?? latestAccountingPeriod?.id ?? "",
    };
    const [fundResponse, balanceEventResponse] = await Promise.all([
      apiClient.GET("/funds/accounting-period-range", {
        params: { query: { ...fundQuery, ...range } },
      }),
      apiClient.GET("/funds/balance-events/accounting-period-range", {
        params: { query: { ...balanceEventQuery, ...range } },
      }),
    ]);
    return {
      trends: unwrapApiResponse(fundResponse, "Failed to load fund trends"),
      balanceEvents: unwrapApiResponse(
        balanceEventResponse,
        "Failed to load fund balance events",
      ),
    };
  })();
  const modeValue: FundTrendsDataMode =
    currentMode === "date" ? "Date" : "AccountingPeriod";
  const periodSummaries =
    "accountingPeriods" in trends ? trends.accountingPeriods : [];
  const dateSummaries = "dates" in trends ? trends.dates : [];
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
  });

  return (
    <PageLayout>
      <ConstrainedContent>
        <FundTrendsFilter
          accountingPeriods={accountingPeriods.items}
          availableFundNames={trends.availableFundNames}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate={defaultStartDate.format("YYYY-MM-DD")}
          defaultEndDate={defaultEndDate.format("YYYY-MM-DD")}
        />
      </ConstrainedContent>
      <FundTrendsSummaryCards
        mode={modeValue}
        accountingPeriods={periodSummaries}
        dates={dateSummaries}
      />
      <FundTrendsAssignmentSpendingCard
        totalAssigned={trends.totalIncome.tracked}
        totalSpent={trends.totalSpending}
      />
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
        <BalanceTrendChart
          chartPoints={balanceTrendChartPoints}
          xAxisLabel={modeValue === "Date" ? "Date" : "Accounting Period"}
        />
        <FundTrendsChangeChart
          mode={modeValue}
          accountingPeriods={periodSummaries}
          dates={dateSummaries}
        />
      </ResponsiveGrid>
      <ResponsiveGrid minimumColumnWidth={800}>
        <FundTrendsListFrame
          data={[...trends.funds.items]}
          isInOnboardingMode={isInOnboardingMode}
          totalCount={trends.funds.totalCount}
        />
        <FundTrendsBalanceEventListFrame
          data={[...balanceEvents.items]}
          mode={modeValue}
          totalCount={balanceEvents.totalCount}
        />
      </ResponsiveGrid>
    </PageLayout>
  );
};

export default FundTrends;
