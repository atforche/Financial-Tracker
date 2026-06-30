"use client";

import type { GoalTrendsAccountingPeriodSummaryModel } from "@/goals/types";
import GoalTrendsMetricChart from "@/goals/trends/GoalTrendsMetricChart";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface GoalTrendsAmountSpentChartProps {
  readonly accountingPeriods:
    readonly GoalTrendsAccountingPeriodSummaryModel[] | null;
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

export default GoalTrendsAmountSpentChart;
