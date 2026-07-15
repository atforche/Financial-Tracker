"use client";

import type { GoalAccountingPeriodSummary , GoalTrendsView } from "@/goals/trends/goalTrendsTypes";
import GoalTrendsMetricChart from "@/goals/trends/GoalTrendsMetricChart";
import type { JSX } from "react";

interface GoalTrendsGoalsMetChartProps {
  readonly accountingPeriods:
    readonly GoalAccountingPeriodSummary[] | null;
  readonly view: GoalTrendsView;
}

/**
 * Charts the percentage of goals met for each accounting period in the selected range.
 */
const GoalTrendsGoalsMetChart = function ({
  accountingPeriods,
  view,
}: GoalTrendsGoalsMetChartProps): JSX.Element {
  return (
    <GoalTrendsMetricChart
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

export default GoalTrendsGoalsMetChart;
