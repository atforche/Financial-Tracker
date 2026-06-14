"use client";

import type { GoalDashboardAccountingPeriodSummaryModel } from "@/goals/types";
import GoalDashboardMetricChart from "@/goals/dashboard/GoalDashboardMetricChart";
import type { GoalDashboardView } from "@/goals/dashboard/goalDashboardTypes";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface GoalDashboardGoalAmountChartProps {
  readonly accountingPeriods:
    | readonly GoalDashboardAccountingPeriodSummaryModel[]
    | null;
  readonly view: GoalDashboardView;
}

/**
 * Charts the primary goal amount metric for each accounting period in the selected range.
 */
const GoalDashboardGoalAmountChart = function ({
  accountingPeriods,
  view,
}: GoalDashboardGoalAmountChartProps): JSX.Element {
  return (
    <GoalDashboardMetricChart
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
