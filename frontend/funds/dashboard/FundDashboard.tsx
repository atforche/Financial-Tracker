import { Box, Stack } from "@mui/material";
import type {
  FundDashboardBalanceEventSortOrder,
  FundDashboardSortOrder,
} from "@/funds/types";
import {
  normalizeRequestedFundNames,
  shouldPersistFundNames,
} from "@/funds/dashboard/fundNameFilter";
import { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import FundDashboardAssignmentSpendingCard from "@/funds/dashboard/FundDashboardAssignmentSpendingCard";
import FundDashboardBalanceEventListFrame from "@/funds/dashboard/FundDashboardBalanceEventListFrame";
import FundDashboardChangeChart from "@/funds/dashboard/FundDashboardChangeChart";
import FundDashboardFilter from "@/funds/dashboard/FundDashboardFilter";
import FundDashboardListFrame from "@/funds/dashboard/FundDashboardListFrame";
import FundDashboardSummaryCards from "@/funds/dashboard/FundDashboardSummaryCards";
import FundDashboardTrendChart from "@/funds/dashboard/FundDashboardTrendChart";
import type { JSX } from "react";
import dayjs from "dayjs";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/funds/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * URL mode values used to filter the Funds dashboard.
 */
type FundsDashboardFilterMode = "accounting-period" | "date";

/**
 * Search parameters for the fund dashboard.
 */
interface FundDashboardSearchParams {
  sort?: FundDashboardSortOrder;
  page?: number | null;
  balanceEventSort?: FundDashboardBalanceEventSortOrder;
  balanceEventPage?: number | string;
  mode?: FundsDashboardFilterMode;
  fundName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
}

/**
 * Props for the FundDashboard component.
 */
interface FundDashboardProps {
  readonly searchParams: Promise<FundDashboardSearchParams>;
}

/**
 * Component that displays the Funds view.
 */
const FundDashboard = async function ({
  searchParams,
}: FundDashboardProps): Promise<JSX.Element> {
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
  const currentMode: FundsDashboardFilterMode =
    typeof mode === "undefined" || isInOnboardingMode ? "date" : mode;
  const currentFundNames = normalizeRequestedFundNames(
    Array.isArray(fundName)
      ? fundName
      : typeof fundName === "string"
        ? [fundName]
        : [],
  );

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
      routes.dashboard({
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
      routes.dashboard({
        mode: "date",
        ...persistedFilters,
        startAccountingPeriodId: latestAccountingPeriod.id,
        endAccountingPeriodId: latestAccountingPeriod.id,
      }),
    );
  }

  const fundDashboardPromise = apiClient.GET("/funds/dashboard", {
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
        ...(typeof page === "number" && page > 0
          ? { Offset: (page - 1) * rowsPerPage }
          : {}),
        ...(typeof balanceEventPage === "number" && balanceEventPage > 0
          ? { BalanceEventOffset: (balanceEventPage - 1) * rowsPerPage }
          : {}),
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
  const { data: dashboard } = await fundDashboardPromise;
  if (typeof dashboard === "undefined") {
    throw new Error("Failed to load fund dashboard data");
  }

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <FundDashboardFilter
          accountingPeriods={accountingPeriods?.items ?? []}
          availableFundNames={dashboard.availableFundNames}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate={defaultStartDate.format("YYYY-MM-DD")}
          defaultEndDate={defaultEndDate.format("YYYY-MM-DD")}
        />
      </Stack>
      <FundDashboardSummaryCards dashboard={dashboard} />
      <FundDashboardAssignmentSpendingCard dashboard={dashboard} />
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
        <FundDashboardTrendChart
          mode={dashboard.mode}
          accountingPeriods={dashboard.accountingPeriods ?? null}
          dates={dashboard.dates ?? null}
        />
        <FundDashboardChangeChart
          mode={dashboard.mode}
          accountingPeriods={dashboard.accountingPeriods ?? null}
          dates={dashboard.dates ?? null}
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
        <FundDashboardListFrame
          data={[...dashboard.funds.items]}
          isInOnboardingMode={isInOnboardingMode}
          totalCount={dashboard.funds.totalCount}
        />
        <FundDashboardBalanceEventListFrame
          data={[...dashboard.balanceEvents.items]}
          mode={dashboard.mode}
          totalCount={dashboard.balanceEvents.totalCount}
        />
      </Box>
    </Stack>
  );
};

export type { FundDashboardSearchParams };
export default FundDashboard;
