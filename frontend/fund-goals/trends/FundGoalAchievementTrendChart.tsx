"use client";

import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { FundGoalTrendPoint } from "@/fund-goals/trends/fundGoalProgressTrends";
import type { JSX } from "react";

/**
 * Props for the FundGoalAchievementTrendChart component.
 */
interface FundGoalAchievementTrendChartProps {
  readonly chartPoints: readonly FundGoalTrendPoint[];
}

/**
 * Renders the percentage of Fund Goals achieved in each Accounting Period.
 */
const FundGoalAchievementTrendChart = function ({
  chartPoints,
}: FundGoalAchievementTrendChartProps): JSX.Element {
  return (
    <BarMetricChart
      title="Goals Achieved"
      emptyMessage="No Fund Goal progress is available for the selected trends range."
      hasData={chartPoints.some((point) => point.configuredGoalCount > 0)}
      chartPoints={chartPoints.map((point) => ({
        tickLabel: point.accountingPeriodName,
        tooltipLabel: point.accountingPeriodName,
        value: point.satisfiedPercentage,
      }))}
      xAxisLabel="Accounting Period"
      yAxisLabel="Goals Achieved"
      tickFormatter={(value) => `${value}%`}
      valueFormatter={(value) => `${value.toFixed(0)}%`}
      getTooltipDescription={(point) => {
        const goal = chartPoints.find(
          (candidate) => candidate.accountingPeriodName === point.tooltipLabel,
        );
        return goal === undefined
          ? ""
          : `${goal.satisfiedGoalCount} of ${goal.configuredGoalCount} goals achieved.`;
      }}
      yAxisDomain={[0, 100]}
    />
  );
};

export default FundGoalAchievementTrendChart;
