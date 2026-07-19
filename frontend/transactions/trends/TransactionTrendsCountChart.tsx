"use client";

import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { BarMetricChartPoint } from "@/framework/charts/barMetricHelpers";
import type { JSX } from "react";
import dayjs from "dayjs";
import { formatLongDate } from "@/framework/dateHelpers";

type TransactionTrendsCountChartMode = "AccountingPeriod" | "Date";

interface TransactionAccountingPeriodSummary {
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly totalCount: number;
}

interface TransactionDateSummary {
  readonly date: string;
  readonly totalCount: number;
}

interface TransactionTrendsCountChartProps {
  readonly mode: TransactionTrendsCountChartMode;
  readonly accountingPeriods:
    readonly TransactionAccountingPeriodSummary[] | null;
  readonly dates: readonly TransactionDateSummary[] | null;
}

const chartColor = "#1976d2";
const countFormatter = new Intl.NumberFormat("en-US", {
  notation: "compact",
  maximumFractionDigits: 1,
});

const buildChartPoints = function (
  mode: TransactionTrendsCountChartMode,
  accountingPeriods: readonly TransactionAccountingPeriodSummary[],
  dates: readonly TransactionDateSummary[],
): BarMetricChartPoint[] {
  if (mode === "AccountingPeriod") {
    return accountingPeriods.map((summary) => ({
      key: summary.accountingPeriodId,
      tickLabel: summary.accountingPeriodName,
      tooltipLabel: summary.accountingPeriodName,
      value: summary.totalCount,
      fill: chartColor,
    }));
  }

  return dates.map((summary) => ({
    key: summary.date,
    tickLabel: dayjs(summary.date).format("MMM D"),
    tooltipLabel: formatLongDate(new Date(`${summary.date}T00:00:00`)),
    value: summary.totalCount,
    fill: chartColor,
  }));
};

/** Renders transaction counts for the Transactions trends. */
const TransactionTrendsCountChart = function ({
  mode,
  accountingPeriods,
  dates,
}: TransactionTrendsCountChartProps): JSX.Element {
  return (
    <BarMetricChart
      title="Transaction Count"
      emptyMessage="No transaction counts are available for the selected trends range."
      chartPoints={buildChartPoints(mode, accountingPeriods ?? [], dates ?? [])}
      xAxisLabel={mode === "Date" ? "Date" : "Accounting Period"}
      yAxisLabel="Transaction Count"
      tickFormatter={(value) => countFormatter.format(value)}
      valueFormatter={(value) => `${countFormatter.format(value)} transactions`}
      getTooltipDescription={({ value }) =>
        value === 1
          ? "1 transaction in this period"
          : "Transactions in this period"
      }
      showZeroLine
    />
  );
};

export default TransactionTrendsCountChart;
