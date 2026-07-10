import { Box, Stack } from "@mui/material";
import type {
  FundTrendsBalanceEventSortOrder,
  FundTrendsSortOrder,
} from "@/funds/types";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import {
  normalizeRequestedFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import { AccountingPeriodSortOrder } from "@/accounting-periods/types";
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
  sort?: FundTrendsSortOrder;
  page?: number | string | null;
  balanceEventSort?: FundTrendsBalanceEventSortOrder;
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
        Search: "",
        Sort: AccountingPeriodSortOrder.DateDescending,
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

  const fundTrendsPromise = apiClient.GET("/funds/trends", {
    params: {
      query: {
        ...(typeof sort === "string" ? { Sort: sort } : {}),
        ...(typeof balanceEventSort === "string"
          ? { BalanceEventSort: balanceEventSort }
          : {}),
        Limit: rowsPerPage,
        BalanceEventLimit: rowsPerPage,
        ...(shouldPersistFundNames(currentFundNames)
          ? { FundName: [...currentFundNames] }
          : {}),
        Offset: getPageOffset(currentPage),
        BalanceEventOffset: getPageOffset(currentBalanceEventPage),
        ...(currentMode === "date"
          ? {
              StartDate:
                typeof startDate === "string"
                  ? startDate
                  : defaultStartDate.format("YYYY-MM-DD"),
              EndDate:
                typeof endDate === "string"
                  ? endDate
                  : defaultEndDate.format("YYYY-MM-DD"),
            }
          : {}),
        ...(currentMode === "accounting-period" &&
        latestAccountingPeriod !== null
          ? {
              StartAccountingPeriodId:
                typeof startAccountingPeriodId === "string"
                  ? startAccountingPeriodId
                  : latestAccountingPeriod.id,
              EndAccountingPeriodId:
                typeof endAccountingPeriodId === "string"
                  ? endAccountingPeriodId
                  : latestAccountingPeriod.id,
            }
          : {}),
      },
    },
  });
  const { data: trends } = await fundTrendsPromise;
  if (typeof trends === "undefined") {
    throw new Error("Failed to load fund trends data");
  }

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
      <FundTrendsSummaryCards trends={trends} />
      <FundTrendsAssignmentSpendingCard trends={trends} />
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
          mode={trends.mode}
          accountingPeriods={trends.accountingPeriods ?? null}
          dates={trends.dates ?? null}
        />
        <FundTrendsChangeChart
          mode={trends.mode}
          accountingPeriods={trends.accountingPeriods ?? null}
          dates={trends.dates ?? null}
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
          data={[...trends.balanceEvents.items]}
          mode={trends.mode}
          totalCount={trends.balanceEvents.totalCount}
        />
      </Box>
    </Stack>
  );
};

export type { FundTrendsSearchParams };
export default FundTrends;
