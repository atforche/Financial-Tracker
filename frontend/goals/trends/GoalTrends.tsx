import {
  AssignmentGoalSortOrder,
  GoalTrendsBalanceEventSortOrder,
  SpendingGoalSortOrder,
} from "@/goals/types";
import { Box, Stack } from "@mui/material";
import {
  type GoalTrendsView,
  defaultGoalTrendsView,
  isGoalTrendsView,
} from "@/goals/trends/goalTrendsTypes";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import GoalTrendsAmountAssignedChart from "@/goals/trends/GoalTrendsAmountAssignedChart";
import GoalTrendsAmountSpentChart from "@/goals/trends/GoalTrendsAmountSpentChart";
import GoalTrendsBalanceEventListFrame from "@/goals/trends/GoalTrendsBalanceEventListFrame";
import GoalTrendsFilter from "@/goals/trends/GoalTrendsFilter";
import GoalTrendsGoalAmountChart from "@/goals/trends/GoalTrendsGoalAmountChart";
import GoalTrendsGoalsMetChart from "@/goals/trends/GoalTrendsGoalsMetChart";
import GoalTrendsListFrame from "@/goals/trends/GoalTrendsListFrame";
import GoalTrendsSummaryCards from "@/goals/trends/GoalTrendsSummaryCards";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import { normalizeGoalTypes } from "@/goals/trends/goalTypeFilter";
import { redirect } from "next/navigation";
import routes from "@/goals/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";
import tryParseEnum from "@/framework/data/tryParseEnum";

interface GoalTrendsSearchParams {
  sort?: string;
  page?: number | string | null;
  balanceEventSort?: GoalTrendsBalanceEventSortOrder;
  balanceEventPage?: number | string | null;
  goalType?: string | string[];
  fundName?: string | string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  view?: GoalTrendsView;
}

interface GoalTrendsProps {
  readonly searchParams: Promise<GoalTrendsSearchParams>;
}

/**
 * Component that displays the Goal trends.
 */
const GoalTrends = async function ({
  searchParams,
}: GoalTrendsProps): Promise<JSX.Element> {
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

  const currentView = isGoalTrendsView(view) ? view : defaultGoalTrendsView;

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
      routes.trends({
        ...(currentView === defaultGoalTrendsView ? {} : { view: currentView }),
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
      ? tryParseEnum(GoalTrendsBalanceEventSortOrder, balanceEventSort)
      : null;
  const currentAssignmentSort =
    currentView === "assignment" && typeof sort === "string"
      ? tryParseEnum(AssignmentGoalSortOrder, sort)
      : null;
  const currentSpendingSort =
    currentView === "spending" && typeof sort === "string"
      ? tryParseEnum(SpendingGoalSortOrder, sort)
      : null;

  const trendsPromise = apiClient.GET("/goals/trends", {
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

  const { data: trends } = await trendsPromise;
  if (typeof trends === "undefined") {
    throw new Error("Failed to load goal trends data");
  }

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <GoalTrendsFilter
          accountingPeriods={accountingPeriods?.items ?? []}
          availableFundNames={trends.availableFundNames}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate=""
          defaultEndDate=""
          view={currentView}
        />
      </Stack>
      <GoalTrendsSummaryCards trends={trends} view={currentView} />
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
        <GoalTrendsGoalAmountChart
          accountingPeriods={trends.accountingPeriods ?? null}
          view={currentView}
        />
        {currentView === "assignment" ? (
          <GoalTrendsAmountAssignedChart
            accountingPeriods={trends.accountingPeriods ?? null}
          />
        ) : (
          <GoalTrendsAmountSpentChart
            accountingPeriods={trends.accountingPeriods ?? null}
          />
        )}
        <GoalTrendsGoalsMetChart
          accountingPeriods={trends.accountingPeriods ?? null}
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
            <GoalTrendsListFrame
              view={currentView}
              data={[...trends.assignmentGoals.items]}
              totalCount={trends.assignmentGoals.totalCount}
              isInOnboardingMode={false}
            />
            <GoalTrendsBalanceEventListFrame
              view={currentView}
              data={[...trends.assignmentBalanceEvents.items]}
              totalCount={trends.assignmentBalanceEvents.totalCount}
            />
          </>
        ) : (
          <>
            <GoalTrendsListFrame
              view={currentView}
              data={[...trends.spendingGoals.items]}
              totalCount={trends.spendingGoals.totalCount}
              isInOnboardingMode={false}
            />
            <GoalTrendsBalanceEventListFrame
              view={currentView}
              data={[...trends.spendingBalanceEvents.items]}
              totalCount={trends.spendingBalanceEvents.totalCount}
            />
          </>
        )}
      </Box>
    </Stack>
  );
};

export type { GoalTrendsSearchParams };
export default GoalTrends;
