"use client";

import {
  formatCompactCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { JSX } from "react";
import { buildRangeMetricChartPoints } from "@/framework/charts/chartPointHelpers";
import { getSignedChartColor } from "@/framework/charts/barMetricHelpers";

/**
 * Mode for the TransactionTrendsAmountChart component, indicating whether to display data by accounting period or by date.
 */
type TransactionTrendsAmountChartMode = "AccountingPeriod" | "Date";

/**
 * Represents a summary of transaction amounts for a specific accounting period.
 */
interface TransactionAccountingPeriodSummary {
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly totalAmount: number;
}

/**
 * Represents a summary of transaction amounts for a specific date.
 */
interface TransactionDateSummary {
  readonly date: string;
  readonly totalAmount: number;
}

/**
 * Props for the TransactionTrendsAmountChart component, which displays transaction amounts over time.
 */
interface TransactionTrendsAmountChartProps {
  readonly mode: TransactionTrendsAmountChartMode;
  readonly accountingPeriods:
    readonly TransactionAccountingPeriodSummary[] | null;
  readonly dates: readonly TransactionDateSummary[] | null;
}

/**
 * Renders transaction amounts for the Transactions trends.
 */
const TransactionTrendsAmountChart = function ({
  mode,
  accountingPeriods,
  dates,
}: TransactionTrendsAmountChartProps): JSX.Element {
  return (
    <BarMetricChart
      title="Transaction Amount"
      emptyMessage="No transaction amounts are available for the selected trends range."
      chartPoints={buildRangeMetricChartPoints({
        mode,
        accountingPeriods: (accountingPeriods ?? []).map((summary) => ({
          name: summary.accountingPeriodName,
          value: summary.totalAmount,
        })),
        dates: (dates ?? []).map((summary) => ({
          date: summary.date,
          value: summary.totalAmount,
        })),
        getColor: getSignedChartColor,
      })}
      xAxisLabel={mode === "Date" ? "Date" : "Accounting Period"}
      yAxisLabel="Transaction Amount"
      tickFormatter={(value) => formatCompactCurrency(value, true)}
      valueFormatter={formatSignedCurrency}
      getTooltipDescription={({ value }) =>
        value > 0
          ? "Net inflow in this period"
          : value < 0
            ? "Net outflow in this period"
            : "No net amount in this period"
      }
      showZeroLine
    />
  );
};

export default TransactionTrendsAmountChart;
