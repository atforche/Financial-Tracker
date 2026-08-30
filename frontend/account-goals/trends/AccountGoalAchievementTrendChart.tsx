"use client";

import type { AccountGoalTrendPoint } from "@/account-goals/trends/accountGoalProgressTrends";
import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { JSX } from "react";

/**
 * Renders Account Goal achievement percentage by Accounting Period.
 */
const AccountGoalAchievementTrendChart = function ({
  chartPoints,
}: {
  readonly chartPoints: readonly AccountGoalTrendPoint[];
}): JSX.Element {
  return (
    <BarMetricChart
      title="Account Goals Achieved"
      emptyMessage="No Account Goal progress is available for the selected trends range."
      hasData={chartPoints.some((point) => point.configuredGoalCount > 0)}
      chartPoints={chartPoints.map((point) => ({
        tickLabel: point.accountingPeriodName,
        tooltipLabel: point.accountingPeriodName,
        value: point.satisfiedPercentage,
      }))}
      xAxisLabel="Accounting Period"
      yAxisLabel="Account Goals Achieved"
      tickFormatter={(value) => `${value}%`}
      valueFormatter={(value) => `${value.toFixed(0)}%`}
      getTooltipDescription={(point) => {
        const match = chartPoints.find(
          (candidate) => candidate.accountingPeriodName === point.tooltipLabel,
        );
        return match === undefined
          ? ""
          : `${match.satisfiedGoalCount} of ${match.configuredGoalCount} Account Goals achieved.`;
      }}
      yAxisDomain={[0, 100]}
    />
  );
};

export default AccountGoalAchievementTrendChart;
