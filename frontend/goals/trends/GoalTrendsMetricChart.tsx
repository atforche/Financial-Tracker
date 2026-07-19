"use client";

import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { GoalAccountingPeriodSummary } from "@/goals/trends/goalTrendsTypes";
import type { JSX } from "react";
import { createAccountingPeriodMetricPoint } from "@/framework/charts/chartPointHelpers";
import { formatCurrency } from "@/framework/currencyHelpers";

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
  const chartPoints = (accountingPeriods ?? []).map((summary) =>
    createAccountingPeriodMetricPoint({
      name: summary.accountingPeriodName,
      value: getValue(summary),
    }),
  );

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
