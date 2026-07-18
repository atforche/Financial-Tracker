"use client";

import type {
  AccountBalanceSummaryByDate,
  AccountBalanceSummaryByPeriod,
} from "@/accounts/types";
import {
  type AccountTrendsDataMode,
  buildChartPoints,
} from "@/accounts/trends/helpers";
import {
  formatCompactCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { JSX } from "react";

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
      chartPoints={buildChartPoints(mode, accountingPeriods, dates)}
      xAxisLabel={mode === "Date" ? "Date" : "Accounting Period"}
      yAxisLabel="Balance Change"
      tickFormatter={(value) => formatCompactCurrency(value, true)}
      valueFormatter={formatSignedCurrency}
      getTooltipDescription={({ value }) =>
        value > 0
          ? "Increase from previous day"
          : value < 0
            ? "Decrease from previous day"
            : "No change from previous day"
      }
      showZeroLine
    />
  );
};

export default AccountTrendsChangeChart;
