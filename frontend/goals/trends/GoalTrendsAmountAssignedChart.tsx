"use client";

import type { GoalAccountingPeriodSummary } from "@/goals/trends/goalTrendsTypes";
import GoalTrendsMetricChart from "@/goals/trends/GoalTrendsMetricChart";
import type { JSX } from "react";
import formatCompactCurrency from "@/framework/formatCompactCurrency";
import formatCurrency from "@/framework/formatCurrency";

interface GoalTrendsAmountAssignedChartProps {
  readonly accountingPeriods: readonly GoalAccountingPeriodSummary[] | null;
}

/**
 * Charts the amount assigned for each accounting period in the selected range.
 */
const GoalTrendsAmountAssignedChart = function ({
  accountingPeriods,
}: GoalTrendsAmountAssignedChartProps): JSX.Element {
  return (
    <GoalTrendsMetricChart
      title="Amount assigned"
      subtitle="No assigned amounts are available for the selected range."
      label="Amount Assigned"
      accountingPeriods={accountingPeriods}
      getValue={(accountingPeriod) => accountingPeriod.totalAmountAssigned}
      formatter={formatCurrency}
      tickFormatter={(value: number) => formatCompactCurrency(value, true)}
    />
  );
};

export default GoalTrendsAmountAssignedChart;
