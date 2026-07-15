import { Box, Stack } from "@mui/material";
import type {
  FundBalanceEventSortValue,
  FundWithBalanceRangeSortValue,
  FundsInAccountingPeriodRange,
  FundsInDateRange,
} from "@/funds/types";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import {
  normalizeRequestedFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import FundTrendsAssignmentSpendingCard from "@/funds/trends/FundTrendsAssignmentSpendingCard";
import FundTrendsBalanceEventListFrame from "@/funds/trends/FundTrendsBalanceEventListFrame";
import FundTrendsChangeChart from "@/funds/trends/FundTrendsChangeChart";
import FundTrendsFilter from "@/funds/trends/FundTrendsFilter";
import FundTrendsListFrame from "@/funds/trends/FundTrendsListFrame";
import FundTrendsSummaryCards from "@/funds/trends/FundTrendsSummaryCards";
import type { JSX } from "react";
import dayjs from "dayjs";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/funds/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";
import { toRepeatedSearchParam } from "@/framework/routes/helpers";

/**
 * URL mode values used to filter the Funds trends.
 */
type FundsTrendsFilterMode = "accounting-period" | "date";

/**
 * Search parameters for the fund trends.
 */
interface FundTrendsSearchParams {
  sort?: FundWithBalanceRangeSortValue;
  page?: number | string | null;
  balanceEventSort?: FundBalanceEventSortValue;
  balanceEventPage?: number | string | null;
  mode?: FundsTrendsFilterMode;
  fundName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
}

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

  const apiClient = getApiClient();
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Sort: "DateDescending",
        Limit: 500,
        Offset: 0,
      },
    },
  });
  const { data: accountingPeriods } = await accountingPeriodsPromise;
  const latestAccountingPeriod = accountingPeriods?.items[0] ?? null;
  const isInOnboardingMode = typeof latestAccountingPeriod === "undefined";
  const currentMode: FundsTrendsFilterMode =
    typeof mode === "undefined" || isInOnboardingMode ? "date" : mode;
  const currentFundNames = normalizeRequestedFundNames(
    toRepeatedSearchParam(fundName),
  );
  const currentPage = normalizePageValue(page);
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
        mode: "date",
        ...persistedFilters,
        startAccountingPeriodId: latestAccountingPeriod.id,
        endAccountingPeriodId: latestAccountingPeriod.id,
      }),
    );
  }

  // Values are assigned by the active range mode below.
  // eslint-disable-next-line @typescript-eslint/init-declarations
  let trends: FundsInDateRange | FundsInAccountingPeriodRange | undefined;
  // eslint-disable-next-line @typescript-eslint/init-declarations
  let balanceEvents;
  const fundQuery = {
    ...(typeof sort === "string" ? { Sort: sort } : {}),
    ...(shouldPersistFundNames(currentFundNames)
      ? { "Filter.Names": [...currentFundNames] }
      : {}),
    Limit: rowsPerPage,
    Offset: getPageOffset(currentPage),
  };
  const balanceEventQuery = {
    ...(typeof balanceEventSort === "string" ? { Sort: balanceEventSort } : {}),
    ...(shouldPersistFundNames(currentFundNames)
      ? { "Filter.Names": [...currentFundNames] }
      : {}),
    Limit: rowsPerPage,
    Offset: getPageOffset(currentBalanceEventPage),
  };
  if (currentMode === "date") {
    const range = {
      "Range.Start": startDate ?? defaultStartDate.format("YYYY-MM-DD"),
      "Range.End": endDate ?? defaultEndDate.format("YYYY-MM-DD"),
    };
    const [fundResponse, balanceEventResponse] = await Promise.all([
      apiClient.GET("/funds/date-range", {
        params: { query: { ...fundQuery, ...range } },
      }),
      apiClient.GET("/balance-events/funds/date-range", {
        params: { query: { ...balanceEventQuery, ...range } },
      }),
    ]);
    trends = fundResponse.data;
    balanceEvents = balanceEventResponse.data;
  } else {
    const range = {
      "Range.Start":
        startAccountingPeriodId ?? latestAccountingPeriod?.id ?? "",
      "Range.End": endAccountingPeriodId ?? latestAccountingPeriod?.id ?? "",
    };
    const [fundResponse, balanceEventResponse] = await Promise.all([
      apiClient.GET("/funds/accounting-period-range", {
        params: { query: { ...fundQuery, ...range } },
      }),
      apiClient.GET("/balance-events/funds/accounting-period-range", {
        params: { query: { ...balanceEventQuery, ...range } },
      }),
    ]);
    trends = fundResponse.data;
    balanceEvents = balanceEventResponse.data;
  }
  if (typeof trends === "undefined" || typeof balanceEvents === "undefined") {
    throw new Error("Failed to load fund trends data");
  }
  const modeValue = currentMode === "date" ? "Date" : "AccountingPeriod";
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

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <FundTrendsFilter
          accountingPeriods={accountingPeriods?.items ?? []}
          availableFundNames={trends.availableFundNames}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate={defaultStartDate.format("YYYY-MM-DD")}
          defaultEndDate={defaultEndDate.format("YYYY-MM-DD")}
        />
      </Stack>
      <FundTrendsSummaryCards
        mode={modeValue}
        accountingPeriods={periodSummaries}
        dates={dateSummaries}
      />
      <FundTrendsAssignmentSpendingCard
        totalAssigned={trends.totalIncome.tracked}
        totalSpent={trends.totalSpending}
      />
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: {
            xs: "1fr",
            lg: "minmax(0, 1fr) minmax(0, 1fr)",
          },
        }}
      >
        <BalanceTrendChart
          mode={modeValue}
          accountingPeriods={chartPeriods}
          dates={dateSummaries}
        />
        <FundTrendsChangeChart
          mode={modeValue}
          accountingPeriods={periodSummaries}
          dates={dateSummaries}
        />
      </Box>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 800px), 1fr))",
        }}
      >
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
      </Box>
    </Stack>
  );
};

export type { FundTrendsSearchParams };
export default FundTrends;
