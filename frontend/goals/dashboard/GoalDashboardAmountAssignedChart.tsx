"use client";

import type { GoalDashboardAccountingPeriodSummaryModel } from "@/goals/types";
import GoalDashboardMetricChart from "@/goals/dashboard/GoalDashboardMetricChart";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface GoalDashboardAmountAssignedChartProps {
  readonly accountingPeriods:
    | readonly GoalDashboardAccountingPeriodSummaryModel[]
    | null;
}

/**
 * Charts the amount assigned for each accounting period in the selected range.
 */
const GoalDashboardAmountAssignedChart = function ({
  accountingPeriods,
}: GoalDashboardAmountAssignedChartProps): JSX.Element {
  return (
    <GoalDashboardMetricChart
      title="Amount assigned"
      subtitle="No assigned amounts are available for the selected range."
      label="Amount Assigned"
      accountingPeriods={accountingPeriods}
      getValue={(accountingPeriod) => accountingPeriod.totalAmountAssigned}
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

export default GoalDashboardAmountAssignedChart;
