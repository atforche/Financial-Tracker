"use client";

import {
  formatCompactCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { JSX } from "react";
import { buildBalanceChangeChartPoints } from "@/framework/charts/balanceChangeHelpers";

/**
 * Props for the BalanceTrendsChangeChart component.
 */
interface BalanceTrendsChangeChartProps {
  readonly mode: "AccountingPeriod" | "Date";
  readonly accountingPeriods: readonly {
    readonly accountingPeriod: { readonly name: string };
    readonly openingBalance: { readonly totalBalance: number };
    readonly closingBalance: { readonly totalBalance: number };
  }[];
  readonly dates: readonly {
    readonly date: string;
    readonly totalBalance: number;
  }[];
  readonly openingBalance: number | null;
  readonly title?: string;
}

/**
 * Renders balance changes for account or fund trends.
 */
const BalanceTrendsChangeChart = function ({
  mode,
  accountingPeriods,
  dates,
  openingBalance,
  title = "Balance Change",
}: BalanceTrendsChangeChartProps): JSX.Element {
  return (
    <BarMetricChart
      title={title}
      emptyMessage="No balance changes are available for the selected trends range."
      chartPoints={buildBalanceChangeChartPoints({
        mode,
        accountingPeriods: accountingPeriods.map((summary) => ({
          name: summary.accountingPeriod.name,
          openingBalance: summary.openingBalance.totalBalance,
          closingBalance: summary.closingBalance.totalBalance,
        })),
        dates: dates.map((summary) => ({
          date: summary.date,
          balance: summary.totalBalance,
        })),
        openingBalance,
      })}
      xAxisLabel={mode === "Date" ? "Date" : "Accounting Period"}
      yAxisLabel="Balance Change"
      tickFormatter={(value) => formatCompactCurrency(value, true)}
      valueFormatter={formatSignedCurrency}
      getTooltipDescription={({ value }) => {
        const comparison = mode === "Date" ? "previous day" : "opening balance";
        return value > 0
          ? `Increase from ${comparison}`
          : value < 0
            ? `Decrease from ${comparison}`
            : `No change from ${comparison}`;
      }}
      showZeroLine
    />
  );
};

export default BalanceTrendsChangeChart;
