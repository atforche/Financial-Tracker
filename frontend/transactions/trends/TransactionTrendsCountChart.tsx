"use client";

import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { JSX } from "react";
import { buildRangeMetricChartPoints } from "@/framework/charts/chartPointHelpers";

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

const countFormatter = new Intl.NumberFormat("en-US", {
  notation: "compact",
  maximumFractionDigits: 1,
});

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
      chartPoints={buildRangeMetricChartPoints({
        mode,
        accountingPeriods: (accountingPeriods ?? []).map((summary) => ({
          name: summary.accountingPeriodName,
          value: summary.totalCount,
        })),
        dates: (dates ?? []).map((summary) => ({
          date: summary.date,
          value: summary.totalCount,
        })),
      })}
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
