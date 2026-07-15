"use client";

import { type BarMetricChartPoint, formatSignedCurrency, getSignedBarColor } from "@/framework/charts/barMetricHelpers";
import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { JSX } from "react";
import dayjs from "dayjs";
import formatCompactCurrency from "@/framework/formatCompactCurrency";
import formatLongDate from "@/framework/formatLongDate";

type TransactionTrendsAmountChartMode = "AccountingPeriod" | "Date";

interface TransactionAccountingPeriodSummary {
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly totalAmount: number;
}

interface TransactionDateSummary {
  readonly date: string;
  readonly totalAmount: number;
}

interface TransactionTrendsAmountChartProps {
  readonly mode: TransactionTrendsAmountChartMode;
  readonly accountingPeriods: readonly TransactionAccountingPeriodSummary[] | null;
  readonly dates: readonly TransactionDateSummary[] | null;
}

const buildChartPoints = function (
  mode: TransactionTrendsAmountChartMode,
  accountingPeriods: readonly TransactionAccountingPeriodSummary[],
  dates: readonly TransactionDateSummary[],
): BarMetricChartPoint[] {
  const summaries = mode === "AccountingPeriod"
    ? accountingPeriods.map((summary) => ({
        key: summary.accountingPeriodId,
        tickLabel: summary.accountingPeriodName,
        tooltipLabel: summary.accountingPeriodName,
        value: summary.totalAmount,
      }))
    : dates.map((summary) => ({
        key: summary.date,
        tickLabel: dayjs(summary.date).format("MMM D"),
        tooltipLabel: formatLongDate(new Date(`${summary.date}T00:00:00`)),
        value: summary.totalAmount,
      }));

  return summaries.map((point) => ({
    ...point,
    fill: getSignedBarColor(point.value),
  }));
};

/** Renders transaction amounts for the Transactions trends. */
const TransactionTrendsAmountChart = function ({ mode, accountingPeriods, dates }: TransactionTrendsAmountChartProps): JSX.Element {
  return (
    <BarMetricChart
      title="Transaction Amount"
      emptyMessage="No transaction amounts are available for the selected trends range."
      chartPoints={buildChartPoints(mode, accountingPeriods ?? [], dates ?? [])}
      xAxisLabel={mode === "Date" ? "Date" : "Accounting Period"}
      yAxisLabel="Transaction Amount"
      tickFormatter={(value) => formatCompactCurrency(value, true)}
      valueFormatter={formatSignedCurrency}
      getTooltipDescription={({ value }) => value > 0
        ? "Net inflow in this period"
        : value < 0
          ? "Net outflow in this period"
          : "No net amount in this period"}
      showZeroLine
    />
  );
};

export default TransactionTrendsAmountChart;
