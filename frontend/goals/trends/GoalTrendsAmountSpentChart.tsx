"use client";

import type { GoalAccountingPeriodSummary } from "@/goals/trends/goalTrendsTypes";
import GoalTrendsMetricChart from "@/goals/trends/GoalTrendsMetricChart";
import type { JSX } from "react";
import formatCompactCurrency from "@/framework/formatCompactCurrency";
import formatCurrency from "@/framework/formatCurrency";

interface GoalTrendsAmountSpentChartProps {
  readonly accountingPeriods:
    readonly GoalAccountingPeriodSummary[] | null;
}

/**
 * Charts the amount spent for each accounting period in the selected range.
 */
const GoalTrendsAmountSpentChart = function ({
  accountingPeriods,
}: GoalTrendsAmountSpentChartProps): JSX.Element {
  return (
    <GoalTrendsMetricChart
      title="Amount spent"
      subtitle="No spending amounts are available for the selected range."
      label="Amount Spent"
      accountingPeriods={accountingPeriods}
      getValue={(accountingPeriod) => accountingPeriod.totalAmountSpent}
      formatter={formatCurrency}
      tickFormatter={(value: number) => formatCompactCurrency(value, true)}
    />
  );
};

export default GoalTrendsAmountSpentChart;
