"use client";

import {
  formatCompactCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { JSX } from "react";
import { buildChangeChartPoints } from "@/accounting-periods/trends/helpers";

/**
 * Props for the AccountingPeriodTrendsChangeChart component.
 */
interface AccountingPeriodTrendsChangeChartProps {
  readonly accountingPeriods: readonly AccountingPeriodWithBalance[] | null;
}

/**
 * Renders balance changes for the Accounting Periods trends.
 */
const AccountingPeriodTrendsChangeChart = function ({
  accountingPeriods,
}: AccountingPeriodTrendsChangeChartProps): JSX.Element {
  return (
    <BarMetricChart
      title="Balance Change"
      emptyMessage="No balance changes are available for the selected trends range."
      chartPoints={buildChangeChartPoints(accountingPeriods ?? [])}
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
