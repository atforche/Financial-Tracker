import { Box, Stack } from "@mui/material";
import type {
  GoalDashboardBalanceEventSortOrder,
  GoalSortOrder,
} from "@/goals/types";
import { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import GoalDashboardAmountAssignedChart from "@/goals/dashboard/GoalDashboardAmountAssignedChart";
import GoalDashboardAmountSpentChart from "@/goals/dashboard/GoalDashboardAmountSpentChart";
import GoalDashboardBalanceEventListFrame from "@/goals/dashboard/GoalDashboardBalanceEventListFrame";
import GoalDashboardFilter from "@/goals/dashboard/GoalDashboardFilter";
import GoalDashboardGoalAmountChart from "@/goals/dashboard/GoalDashboardGoalAmountChart";
import GoalDashboardGoalsMetChart from "@/goals/dashboard/GoalDashboardGoalsMetChart";
import GoalDashboardListFrame from "@/goals/dashboard/GoalDashboardListFrame";
import GoalDashboardSummaryCards from "@/goals/dashboard/GoalDashboardSummaryCards";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import { normalizeGoalTypes } from "@/goals/dashboard/goalTypeFilter";
import { redirect } from "next/navigation";
import routes from "@/goals/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

interface GoalDashboardSearchParams {
  sort?: GoalSortOrder;
  page?: number | null;
  balanceEventSort?: GoalDashboardBalanceEventSortOrder;
  balanceEventPage?: number | string;
  goalType?: string | string[];
  fundName?: string | string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
}

interface GoalDashboardProps {
  readonly searchParams: Promise<GoalDashboardSearchParams>;
}

/**
 * Component that displays the Goal dashboard.
 */
const GoalDashboard = async function ({
  searchParams,
}: GoalDashboardProps): Promise<JSX.Element> {
  const {
    sort,
    page,
    balanceEventSort,
    balanceEventPage,
    goalType,
    fundName,
    startAccountingPeriodId,
    endAccountingPeriodId,
  } = await searchParams;

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

  if (latestAccountingPeriod === null) {
    throw new Error("Failed to load goal dashboard data");
  }

  if (
    typeof startAccountingPeriodId === "undefined" ||
    typeof endAccountingPeriodId === "undefined"
  ) {
    redirect(
      routes.dashboard({
        startAccountingPeriodId: latestAccountingPeriod.id,
        endAccountingPeriodId: latestAccountingPeriod.id,
      }),
    );
  }

  const currentGoalTypes = normalizeGoalTypes(
    Array.isArray(goalType)
      ? goalType
      : typeof goalType === "string"
        ? [goalType]
        : [],
  );

  const getPageOffset = function (
    value: number | string | null | undefined,
  ): number | null {
    const currentPage = Number.parseInt(String(value), 10);
    return Number.isNaN(currentPage) || currentPage <= 0
      ? null
      : (currentPage - 1) * rowsPerPage;
  };

  const pageOffset =
    typeof page === "string" || typeof page === "number"
      ? getPageOffset(page)
      : null;
  const balanceEventOffset =
    typeof balanceEventPage === "string" || typeof balanceEventPage === "number"
      ? getPageOffset(balanceEventPage)
      : null;

  const dashboardPromise = apiClient.GET("/goals/dashboard", {
    params: {
      query: {
        ...(typeof sort === "string" ? { Sort: sort } : {}),
        ...(typeof balanceEventSort === "string"
          ? { BalanceEventSort: balanceEventSort }
          : {}),
        Limit: rowsPerPage,
        BalanceEventLimit: rowsPerPage,
        ...(pageOffset === null ? {} : { Offset: pageOffset }),
        ...(balanceEventOffset === null
          ? {}
          : { BalanceEventOffset: balanceEventOffset }),
        StartAccountingPeriodId:
          typeof startAccountingPeriodId === "string"
            ? startAccountingPeriodId
            : latestAccountingPeriod.id,
        EndAccountingPeriodId:
          typeof endAccountingPeriodId === "string"
            ? endAccountingPeriodId
            : latestAccountingPeriod.id,
        ...(currentGoalTypes.length > 0
          ? { GoalType: [...currentGoalTypes] }
          : {}),
        ...(Array.isArray(fundName)
          ? { FundName: [...fundName] }
          : typeof fundName === "string"
            ? { FundName: [fundName] }
            : {}),
      },
    },
  });

  const { data: dashboard } = await dashboardPromise;
  if (typeof dashboard === "undefined") {
    throw new Error("Failed to load goal dashboard data");
  }

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <GoalDashboardFilter
          accountingPeriods={accountingPeriods?.items ?? []}
          availableFundNames={dashboard.availableFundNames}
          defaultAccountingPeriodId={latestAccountingPeriod.id}
          defaultStartDate=""
          defaultEndDate=""
        />
      </Stack>
      <GoalDashboardSummaryCards dashboard={dashboard} />
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: {
            xs: "1fr",
            lg: "repeat(2, minmax(0, 1fr))",
          },
        }}
      >
        <GoalDashboardGoalAmountChart
          accountingPeriods={dashboard.accountingPeriods ?? null}
        />
        <GoalDashboardAmountAssignedChart
          accountingPeriods={dashboard.accountingPeriods ?? null}
        />
        <GoalDashboardAmountSpentChart
          accountingPeriods={dashboard.accountingPeriods ?? null}
        />
        <GoalDashboardGoalsMetChart
          accountingPeriods={dashboard.accountingPeriods ?? null}
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
        <GoalDashboardListFrame
          data={[...dashboard.goals.items]}
          totalCount={dashboard.goals.totalCount}
          isInOnboardingMode={false}
        />
        <GoalDashboardBalanceEventListFrame
          data={[...dashboard.balanceEvents.items]}
          totalCount={dashboard.balanceEvents.totalCount}
        />
      </Box>
    </Stack>
  );
};

export type { GoalDashboardSearchParams };
export default GoalDashboard;
