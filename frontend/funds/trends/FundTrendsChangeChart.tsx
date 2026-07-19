"use client";

import type {
  FundBalanceSummaryByDate,
  FundBalanceSummaryByPeriod,
} from "@/funds/types";
import {
  formatCompactCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { JSX } from "react";
import { buildBalanceChangeChartPoints } from "@/framework/charts/balanceChangeHelpers";

type FundTrendsChangeChartMode = "AccountingPeriod" | "Date";

interface FundTrendsChangeChartProps {
  readonly mode: FundTrendsChangeChartMode;
  readonly accountingPeriods: readonly FundBalanceSummaryByPeriod[] | null;
  readonly dates: readonly FundBalanceSummaryByDate[] | null;
}

/** Renders balance changes for the fund trends. */
const FundTrendsChangeChart = function ({
  mode,
  accountingPeriods,
  dates,
}: FundTrendsChangeChartProps): JSX.Element {
  return (
    <BarMetricChart
      title="Balance Change"
      emptyMessage="No balance changes are available for the selected trends range."
      chartPoints={buildBalanceChangeChartPoints({
        mode,
        accountingPeriods: (accountingPeriods ?? []).map((summary) => ({
          name: summary.accountingPeriod.name,
          openingBalance: summary.openingBalance.totalBalance,
          closingBalance: summary.closingBalance.totalBalance,
        })),
        dates: (dates ?? []).map((summary) => ({
          date: summary.date,
          balance: summary.totalBalance,
        })),
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

export default FundTrendsChangeChart;
