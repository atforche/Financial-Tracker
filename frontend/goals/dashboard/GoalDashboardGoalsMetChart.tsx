"use client";

import type { GoalDashboardAccountingPeriodSummaryModel } from "@/goals/types";
import GoalDashboardMetricChart from "@/goals/dashboard/GoalDashboardMetricChart";
import type { GoalDashboardView } from "@/goals/dashboard/goalDashboardTypes";
import type { JSX } from "react";

interface GoalDashboardGoalsMetChartProps {
  readonly accountingPeriods:
    | readonly GoalDashboardAccountingPeriodSummaryModel[]
    | null;
  readonly view: GoalDashboardView;
}

/**
 * Charts the percentage of goals met for each accounting period in the selected range.
 */
const GoalDashboardGoalsMetChart = function ({
  accountingPeriods,
  view,
}: GoalDashboardGoalsMetChartProps): JSX.Element {
  return (
    <GoalDashboardMetricChart
      title="Goals met"
      subtitle="No goals-met percentages are available for the selected range."
      label="Goals Met (%)"
      accountingPeriods={accountingPeriods}
      getValue={(accountingPeriod) =>
        view === "assignment"
          ? accountingPeriod.percentageOfAssignmentGoalsMet.percentageMet
          : accountingPeriod.percentageOfSpendingGoalsMet.percentageMet
      }
      formatter={(value: number) => `${value.toFixed(2)}%`}
      tickFormatter={(value: number) => `${value.toFixed(0)}%`}
    />
  );
};

export default GoalDashboardGoalsMetChart;
