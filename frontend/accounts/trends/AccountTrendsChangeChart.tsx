"use client";

import type {
  AccountBalanceSummaryByDate,
  AccountBalanceSummaryByPeriod,
} from "@/accounts/types";
import {
  formatCompactCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import type { AccountTrendsDataMode } from "@/accounts/trends/helpers";
import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { JSX } from "react";
import { buildBalanceChangeChartPoints } from "@/framework/charts/balanceChangeHelpers";

/**
 * Props for the AccountTrendsChangeChart component.
 */
interface AccountTrendsChangeChartProps {
  readonly mode: AccountTrendsDataMode;
  readonly accountingPeriods: readonly AccountBalanceSummaryByPeriod[];
  readonly dates: readonly AccountBalanceSummaryByDate[];
}

/**
 * Renders balance changes for the account trends.
 */
const AccountTrendsChangeChart = function ({
  mode,
  accountingPeriods,
  dates,
}: AccountTrendsChangeChartProps): JSX.Element {
  return (
    <BarMetricChart
      title="Balance Change"
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

export default AccountTrendsChangeChart;
