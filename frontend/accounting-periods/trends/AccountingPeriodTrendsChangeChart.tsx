"use client";

import {
  type BarMetricChartPoint,
  formatSignedCurrency,
  getSignedBarColor,
} from "@/framework/charts/barMetricHelpers";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { JSX } from "react";
import formatCompactCurrency from "@/framework/formatCompactCurrency";

interface AccountingPeriodTrendsChangeChartProps {
  readonly accountingPeriods: readonly AccountingPeriodWithBalance[] | null;
}

const buildChartPoints = function (
  accountingPeriods: readonly AccountingPeriodWithBalance[],
): BarMetricChartPoint[] {
  return accountingPeriods.map((accountingPeriod) => {
    const value =
      accountingPeriod.closingBalance - accountingPeriod.openingBalance;

    return {
      key: accountingPeriod.id,
      tickLabel: accountingPeriod.name,
      tooltipLabel: accountingPeriod.name,
      value,
      fill: getSignedBarColor(value),
    };
  });
};

/** Renders balance changes for the Accounting Periods trends. */
const AccountingPeriodTrendsChangeChart = function ({
  accountingPeriods,
}: AccountingPeriodTrendsChangeChartProps): JSX.Element {
  return (
    <BarMetricChart
      title="Balance Change"
      emptyMessage="No balance changes are available for the selected trends range."
      chartPoints={buildChartPoints(accountingPeriods ?? [])}
      xAxisLabel="Accounting Period"
      yAxisLabel="Balance Change"
      tickFormatter={(value) => formatCompactCurrency(value, true)}
      valueFormatter={formatSignedCurrency}
      getTooltipDescription={({ value }) =>
        value > 0
          ? "Increase from opening balance"
          : value < 0
            ? "Decrease from opening balance"
            : "No change from opening balance"
      }
      showZeroLine
    />
  );
};

export default AccountingPeriodTrendsChangeChart;
