import {
  AssignmentGoalSortOrder,
  GoalDashboardBalanceEventSortOrder,
  SpendingGoalSortOrder,
} from "@/goals/types";
import { Box, Stack } from "@mui/material";
import {
  type GoalDashboardView,
  defaultGoalDashboardView,
  isGoalDashboardView,
} from "@/goals/dashboard/goalDashboardTypes";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
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
import tryParseEnum from "@/framework/data/tryParseEnum";

interface GoalDashboardSearchParams {
  sort?: string;
  page?: number | string | null;
  balanceEventSort?: GoalDashboardBalanceEventSortOrder;
  balanceEventPage?: number | string | null;
  goalType?: string | string[];
  fundName?: string | string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  view?: GoalDashboardView;
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
    view,
  } = await searchParams;

  const currentView = isGoalDashboardView(view)
    ? view
    : defaultGoalDashboardView;

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

  if (
    (typeof startAccountingPeriodId === "undefined" ||
      typeof endAccountingPeriodId === "undefined") &&
    latestAccountingPeriod !== null
  ) {
    redirect(
      routes.dashboard({
        ...(currentView === defaultGoalDashboardView
          ? {}
          : { view: currentView }),
        startAccountingPeriodId: latestAccountingPeriod.id,
        endAccountingPeriodId: latestAccountingPeriod.id,
      }),
    );
  }

  const requestedGoalTypes = Array.isArray(goalType)
    ? goalType
    : typeof goalType === "string"
      ? [goalType]
      : [];
  const currentAssignmentGoalTypes =
    currentView === "assignment"
      ? normalizeGoalTypes(requestedGoalTypes, currentView)
      : [];
  const currentSpendingGoalTypes =
    currentView === "spending"
      ? normalizeGoalTypes(requestedGoalTypes, currentView)
      : [];
  const currentPage = normalizePageValue(page);
  const currentBalanceEventPage = normalizePageValue(balanceEventPage);
  const pageOffset = getPageOffset(currentPage);
  const balanceEventOffset = getPageOffset(currentBalanceEventPage);
  const currentBalanceEventSort =
    typeof balanceEventSort === "string"
      ? tryParseEnum(GoalDashboardBalanceEventSortOrder, balanceEventSort)
      : null;
  const currentAssignmentSort =
    currentView === "assignment" && typeof sort === "string"
      ? tryParseEnum(AssignmentGoalSortOrder, sort)
      : null;
  const currentSpendingSort =
    currentView === "spending" && typeof sort === "string"
      ? tryParseEnum(SpendingGoalSortOrder, sort)
      : null;

  const dashboardPromise = apiClient.GET("/goals/dashboard", {
    params: {
      query: {
        AssignmentGoalLimit: currentView === "assignment" ? rowsPerPage : 0,
        AssignmentGoalOffset: currentView === "assignment" ? pageOffset : 0,
        SpendingGoalLimit: currentView === "spending" ? rowsPerPage : 0,
        SpendingGoalOffset: currentView === "spending" ? pageOffset : 0,
        AssignmentBalanceEventLimit:
          currentView === "assignment" ? rowsPerPage : 0,
        AssignmentBalanceEventOffset:
          currentView === "assignment" ? balanceEventOffset : 0,
        SpendingBalanceEventLimit: currentView === "spending" ? rowsPerPage : 0,
        SpendingBalanceEventOffset:
          currentView === "spending" ? balanceEventOffset : 0,
        ...(typeof startAccountingPeriodId === "string"
          ? { StartAccountingPeriodId: startAccountingPeriodId }
          : latestAccountingPeriod !== null
            ? { StartAccountingPeriodId: latestAccountingPeriod.id }
            : {}),
        ...(typeof endAccountingPeriodId === "string"
          ? { EndAccountingPeriodId: endAccountingPeriodId }
          : latestAccountingPeriod !== null
            ? { EndAccountingPeriodId: latestAccountingPeriod.id }
            : {}),
        ...(currentAssignmentGoalTypes.length > 0
          ? {
              AssignmentGoalType: [...currentAssignmentGoalTypes],
            }
          : {}),
        ...(currentSpendingGoalTypes.length > 0
          ? {
              SpendingGoalType: [...currentSpendingGoalTypes],
            }
          : {}),
        ...(Array.isArray(fundName)
          ? { FundName: [...fundName] }
          : typeof fundName === "string"
            ? { FundName: [fundName] }
            : {}),
        ...(currentAssignmentSort !== null
          ? { AssignmentSort: currentAssignmentSort }
          : {}),
        ...(currentSpendingSort !== null
          ? { SpendingSort: currentSpendingSort }
          : {}),
        ...(currentView === "assignment" && currentBalanceEventSort !== null
          ? { AssignmentBalanceEventSort: currentBalanceEventSort }
          : {}),
        ...(currentView === "spending" && currentBalanceEventSort !== null
          ? { SpendingBalanceEventSort: currentBalanceEventSort }
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
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate=""
          defaultEndDate=""
          view={currentView}
        />
      </Stack>
      <GoalDashboardSummaryCards dashboard={dashboard} view={currentView} />
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: {
            xs: "1fr",
            lg: "repeat(3, minmax(0, 1fr))",
          },
        }}
      >
        <GoalDashboardGoalAmountChart
          accountingPeriods={dashboard.accountingPeriods ?? null}
          view={currentView}
        />
        {currentView === "assignment" ? (
          <GoalDashboardAmountAssignedChart
            accountingPeriods={dashboard.accountingPeriods ?? null}
          />
        ) : (
          <GoalDashboardAmountSpentChart
            accountingPeriods={dashboard.accountingPeriods ?? null}
          />
        )}
        <GoalDashboardGoalsMetChart
          accountingPeriods={dashboard.accountingPeriods ?? null}
          view={currentView}
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
        {currentView === "assignment" ? (
          <>
            <GoalDashboardListFrame
              view={currentView}
              data={[...dashboard.assignmentGoals.items]}
              totalCount={dashboard.assignmentGoals.totalCount}
              isInOnboardingMode={false}
            />
            <GoalDashboardBalanceEventListFrame
              view={currentView}
              data={[...dashboard.assignmentBalanceEvents.items]}
              totalCount={dashboard.assignmentBalanceEvents.totalCount}
            />
          </>
        ) : (
          <>
            <GoalDashboardListFrame
              view={currentView}
              data={[...dashboard.spendingGoals.items]}
              totalCount={dashboard.spendingGoals.totalCount}
              isInOnboardingMode={false}
            />
            <GoalDashboardBalanceEventListFrame
              view={currentView}
              data={[...dashboard.spendingBalanceEvents.items]}
              totalCount={dashboard.spendingBalanceEvents.totalCount}
            />
          </>
        )}
      </Box>
    </Stack>
  );
};

export type { GoalDashboardSearchParams };
export default GoalDashboard;
