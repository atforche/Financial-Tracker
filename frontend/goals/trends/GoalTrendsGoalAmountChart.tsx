"use client";

import type {
  GoalAccountingPeriodSummary,
  GoalTrendsView,
} from "@/goals/trends/goalTrendsTypes";
import {
  formatCompactCurrency,
  formatCurrency,
} from "@/framework/currencyHelpers";
import GoalTrendsMetricChart from "@/goals/trends/GoalTrendsMetricChart";
import type { JSX } from "react";

/**
 * Props for the GoalTrendsGoalAmountChart component.
 */
interface GoalTrendsGoalAmountChartProps {
  readonly accountingPeriods: readonly GoalAccountingPeriodSummary[] | null;
  readonly view: GoalTrendsView;
}

/**
 * Charts the primary goal amount metric for each accounting period in the selected range.
 */
const GoalTrendsGoalAmountChart = function ({
  accountingPeriods,
  view,
}: GoalTrendsGoalAmountChartProps): JSX.Element {
  return (
    <GoalTrendsMetricChart
      title={view === "assignment" ? "Amount to assign" : "Amount to spend"}
      subtitle={
        view === "assignment"
          ? "No assignment goal amounts are available for the selected range."
          : "No spending goal amounts are available for the selected range."
      }
      label={view === "assignment" ? "Amount To Assign" : "Amount To Spend"}
      accountingPeriods={accountingPeriods}
      getValue={(accountingPeriod) =>
        view === "assignment"
          ? accountingPeriod.totalAmountToAssign
          : accountingPeriod.totalAmountToSpend
      }
      formatter={formatCurrency}
      tickFormatter={(value: number) => formatCompactCurrency(value, true)}
    />
  );
};

export default GoalTrendsGoalAmountChart;
