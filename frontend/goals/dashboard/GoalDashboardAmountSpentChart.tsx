"use client";

import type { GoalDashboardAccountingPeriodSummaryModel } from "@/goals/types";
import GoalDashboardMetricChart from "@/goals/dashboard/GoalDashboardMetricChart";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface GoalDashboardAmountSpentChartProps {
  readonly accountingPeriods:
    | readonly GoalDashboardAccountingPeriodSummaryModel[]
    | null;
}

/**
 * Charts the amount spent for each accounting period in the selected range.
 */
const GoalDashboardAmountSpentChart = function ({
  accountingPeriods,
}: GoalDashboardAmountSpentChartProps): JSX.Element {
  return (
    <GoalDashboardMetricChart
      title="Amount spent"
      subtitle="No spending amounts are available for the selected range."
      label="Amount Spent"
      accountingPeriods={accountingPeriods}
      getValue={(accountingPeriod) => accountingPeriod.totalAmountSpent}
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

export default GoalDashboardAmountSpentChart;
