"use client";

import type { GoalTrendsAccountingPeriodSummaryModel } from "@/goals/types";
import GoalTrendsMetricChart from "@/goals/trends/GoalTrendsMetricChart";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface GoalTrendsAmountAssignedChartProps {
  readonly accountingPeriods:
    readonly GoalTrendsAccountingPeriodSummaryModel[] | null;
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

export default GoalTrendsAmountAssignedChart;
