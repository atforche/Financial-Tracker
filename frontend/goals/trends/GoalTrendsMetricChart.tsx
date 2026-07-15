"use client";

import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { GoalAccountingPeriodSummary } from "@/goals/trends/goalTrendsTypes";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface GoalTrendsMetricChartProps {
  readonly title: string;
  readonly subtitle: string;
  readonly label: string;
  readonly accountingPeriods: readonly GoalAccountingPeriodSummary[] | null;
  readonly getValue: (accountingPeriod: GoalAccountingPeriodSummary) => number;
  readonly formatter: (value: number) => string;
  readonly tickFormatter?: (value: number) => string;
}

/** Displays a single metric across accounting periods on the goal trends. */
const GoalTrendsMetricChart = function ({
  title,
  subtitle,
  label,
  accountingPeriods,
  getValue,
  formatter,
  tickFormatter,
}: GoalTrendsMetricChartProps): JSX.Element {
  const chartPoints = (accountingPeriods ?? []).map((summary) => ({
    key: summary.accountingPeriodId,
    tickLabel: summary.accountingPeriodName,
    tooltipLabel: summary.accountingPeriodName,
    value: getValue(summary),
    fill: "#1976d2",
  }));

  return (
    <BarMetricChart
      title={title}
      emptyMessage={subtitle}
      chartPoints={chartPoints}
      xAxisLabel="Accounting Period"
      yAxisLabel={label}
      tickFormatter={tickFormatter ?? formatCurrency}
      valueFormatter={formatter}
    />
  );
};

export default GoalTrendsMetricChart;
