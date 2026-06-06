"use client";

import type { GoalDashboardAccountingPeriodSummaryModel } from "@/goals/types";
import GoalDashboardMetricChart from "@/goals/dashboard/GoalDashboardMetricChart";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface GoalDashboardGoalAmountChartProps {
  readonly accountingPeriods:
    | readonly GoalDashboardAccountingPeriodSummaryModel[]
    | null;
}

/**
 * Charts the goal amount for each accounting period in the selected range.
 */
const GoalDashboardGoalAmountChart = function ({
  accountingPeriods,
}: GoalDashboardGoalAmountChartProps): JSX.Element {
  return (
    <GoalDashboardMetricChart
      title="Goal amount"
      subtitle="No goal amounts are available for the selected range."
      dataKey="goalAmount"
      label="Goal Amount"
      accountingPeriods={accountingPeriods}
      formatter={formatCurrency}
      tickFormatter={(value: number) =>
        new Intl.NumberFormat("en-US", {
          style: "currency",
          currency: "USD",
          notation: "compact",
          maximumFractionDigits: 1,
          signDisplay: "exceptZero",
        }).format(value)
      }
    />
  );
};

export default GoalDashboardGoalAmountChart;
