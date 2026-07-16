import {
  AssignmentGoalSort,
  GoalBalanceEventSort,
  type GoalBalanceEventSortValue,
  SpendingGoalSort,
} from "@/goals/types";
import {
  type GoalTrendsView,
  defaultGoalTrendsView,
  isGoalTrendsView,
} from "@/goals/trends/goalTrendsTypes";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import {
  summarizeGoalRange,
  summarizeGoalsByAccountingPeriod,
} from "@/goals/trends/goalTrendsSummary";
import { BalanceEventTypeModel } from "@/framework/data/api";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import GoalTrendsAmountAssignedChart from "@/goals/trends/GoalTrendsAmountAssignedChart";
import GoalTrendsAmountSpentChart from "@/goals/trends/GoalTrendsAmountSpentChart";
import GoalTrendsBalanceEventListFrame from "@/goals/trends/GoalTrendsBalanceEventListFrame";
import GoalTrendsFilter from "@/goals/trends/GoalTrendsFilter";
import GoalTrendsGoalAmountChart from "@/goals/trends/GoalTrendsGoalAmountChart";
import GoalTrendsGoalsMetChart from "@/goals/trends/GoalTrendsGoalsMetChart";
import GoalTrendsListFrame from "@/goals/trends/GoalTrendsListFrame";
import GoalTrendsSummaryCards from "@/goals/trends/GoalTrendsSummaryCards";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import getApiClient from "@/framework/data/getApiClient";
import getApiData from "@/framework/data/apiResponse";
import { normalizeGoalTypes } from "@/goals/trends/goalTypeFilter";
import { redirect } from "next/navigation";
import routes from "@/goals/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";
import { toRepeatedSearchParam } from "@/framework/routes/helpers";
import tryParseEnum from "@/framework/data/tryParseEnum";

interface GoalTrendsSearchParams {
  sort?: string;
  page?: number | string | null;
  balanceEventSort?: GoalBalanceEventSortValue;
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
 * Component that displays Goal trends.
 */
const GoalTrends = async function ({
  searchParams,
}: GoalTrendsProps): Promise<JSX.Element> {
  const params = await searchParams;
  const currentView = isGoalTrendsView(params.view)
    ? params.view
    : defaultGoalTrendsView;
  const apiClient = getApiClient();
  const accountingPeriodsResponse = await apiClient.GET(
    "/accounting-periods",
    {
      params: { query: { Sort: "DateDescending", Limit: 500, Offset: 0 } },
    },
  );
  const accountingPeriods = getApiData(accountingPeriodsResponse, "Failed to load accounting periods");
  const latestAccountingPeriod = accountingPeriods.items[0] ?? null;
  if (
    (typeof params.startAccountingPeriodId === "undefined" ||
      typeof params.endAccountingPeriodId === "undefined") &&
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

  const startId = params.startAccountingPeriodId ?? latestAccountingPeriod?.id;
  const endId = params.endAccountingPeriodId ?? latestAccountingPeriod?.id;
  const assignmentSort =
    currentView === "assignment" && typeof params.sort === "string"
      ? tryParseEnum(AssignmentGoalSort, params.sort)
      : null;
  const spendingSort =
    currentView === "spending" && typeof params.sort === "string"
      ? tryParseEnum(SpendingGoalSort, params.sort)
      : null;
  const range = {
    ...(typeof startId === "string" ? { "Range.Start": startId } : {}),
    ...(typeof endId === "string" ? { "Range.End": endId } : {}),
  };
  const balanceEventSort =
    typeof params.balanceEventSort === "string"
      ? tryParseEnum(GoalBalanceEventSort, params.balanceEventSort)
      : null;
  const [
    periodResponse,
    assignmentResponse,
    spendingResponse,
    balanceEventResponse,
  ] = await Promise.all([
    apiClient.GET("/accounting-periods/range", {
      params: {
        query: { ...range, Sort: "Date" as const, Limit: 500, Offset: 0 },
      },
    }),
    apiClient.GET("/goals/assignment", {
      params: {
        query: {
          "Filter.AccountingPeriodIds": accountingPeriods.items.map(
            (period) => period.id,
          ),
          ...(assignmentSort === null ? {} : { Sort: assignmentSort }),
          Limit: 500,
          Offset: 0,
        },
      },
    }),
    apiClient.GET("/goals/spending", {
      params: {
        query: {
          "Filter.AccountingPeriodIds": accountingPeriods.items.map(
            (period) => period.id,
          ),
          ...(spendingSort === null ? {} : { Sort: spendingSort }),
          Limit: 500,
          Offset: 0,
        },
      },
    }),
    apiClient.GET("/balance-events/goals/accounting-period-range", {
      params: {
        query: {
          ...range,
          ...(balanceEventSort === null ? {} : { Sort: balanceEventSort }),
          Limit: 500,
          Offset: 0,
        },
      },
    }),
  ]);
  const periodData = getApiData(periodResponse, "Failed to load accounting period trends");
  const assignmentData = getApiData(assignmentResponse, "Failed to load assignment goals");
  const spendingData = getApiData(spendingResponse, "Failed to load spending goals");
  const balanceEventData = getApiData(balanceEventResponse, "Failed to load goal balance events");

  const periodIds = new Set(
    periodData.accountingPeriods.items.map((period) => period.id),
  );
  const fundNames = toRepeatedSearchParam(params.fundName);
  const requestedTypes = toRepeatedSearchParam(params.goalType);
  const assignmentTypes =
    currentView === "assignment"
      ? normalizeGoalTypes(requestedTypes, currentView)
      : [];
  const spendingTypes =
    currentView === "spending"
      ? normalizeGoalTypes(requestedTypes, currentView)
      : [];
  const assignmentGoals = assignmentData.items.filter(
    (goal) =>
      (goal.accountingPeriod === null ||
        periodIds.has(goal.accountingPeriod.id)) &&
      (fundNames.length === 0 || fundNames.includes(goal.fund.name)) &&
      (assignmentTypes.length === 0 || assignmentTypes.includes(goal.type)),
  );
  const spendingGoals = spendingData.items.filter(
    (goal) =>
      (goal.accountingPeriod === null ||
        periodIds.has(goal.accountingPeriod.id)) &&
      (fundNames.length === 0 || fundNames.includes(goal.fund.name)) &&
      (spendingTypes.length === 0 || spendingTypes.includes(goal.type)),
  );
  const summary = summarizeGoalRange(assignmentGoals, spendingGoals);
  const periodSummaries = summarizeGoalsByAccountingPeriod(
    periodData.accountingPeriods.items,
    assignmentGoals,
    spendingGoals,
  );
  const eventType =
    currentView === "assignment"
      ? BalanceEventTypeModel.Credit
      : BalanceEventTypeModel.Debit;
  const events = balanceEventData.items.filter(
    (event) =>
      event.type === eventType &&
      (fundNames.length === 0 || fundNames.includes(event.fund.name)),
  );
  const pageOffset = getPageOffset(normalizePageValue(params.page));
  const eventOffset = getPageOffset(
    normalizePageValue(params.balanceEventPage),
  );
  const currentGoals =
    currentView === "assignment" ? assignmentGoals : spendingGoals;
  const availableFundNames = Array.from(
    new Set(
      [...assignmentData.items, ...spendingData.items].map(
        (goal) => goal.fund.name,
      ),
    ),
  ).sort();

  return (
    <PageLayout>
      <ConstrainedContent>
        <GoalTrendsFilter
          accountingPeriods={accountingPeriods.items}
          availableFundNames={availableFundNames}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate=""
          defaultEndDate=""
          view={currentView}
        />
      </ConstrainedContent>
      <GoalTrendsSummaryCards trends={summary} view={currentView} />
      <ResponsiveGrid columns={{ xs: 1, lg: 3 }}>
        <GoalTrendsGoalAmountChart
          accountingPeriods={periodSummaries}
          view={currentView}
        />
        {currentView === "assignment" ? (
          <GoalTrendsAmountAssignedChart accountingPeriods={periodSummaries} />
        ) : (
          <GoalTrendsAmountSpentChart accountingPeriods={periodSummaries} />
        )}
        <GoalTrendsGoalsMetChart
          accountingPeriods={periodSummaries}
          view={currentView}
        />
      </ResponsiveGrid>
      <ResponsiveGrid minimumColumnWidth={800}>
        <GoalTrendsListFrame
          view={currentView}
          data={currentGoals.slice(pageOffset, pageOffset + rowsPerPage)}
          totalCount={currentGoals.length}
          isInOnboardingMode={false}
        />
        <GoalTrendsBalanceEventListFrame
          view={currentView}
          data={events.slice(eventOffset, eventOffset + rowsPerPage)}
          totalCount={events.length}
        />
      </ResponsiveGrid>
    </PageLayout>
  );
};

export type { GoalTrendsSearchParams };
export default GoalTrends;
