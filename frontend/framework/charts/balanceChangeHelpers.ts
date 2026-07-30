import {
  type BarMetricChartPoint,
  getSignedChartColor,
} from "@/framework/charts/barMetricHelpers";
import {
  createAccountingPeriodMetricPoint,
  createDateMetricPoint,
} from "@/framework/charts/chartPointHelpers";
import type { ChartRangeMode } from "@/framework/charts/chartTypes";
import { getCurrencyDifference } from "@/framework/currencyHelpers";

/**
 * Type representing a summary of balances for a specific accounting period or date.
 */
interface PeriodBalanceSummary {
  readonly name: string;
  readonly openingBalance: number;
  readonly closingBalance: number;
}

/**
 * Type representing a summary of balances for a specific date.
 */
interface DateBalanceSummary {
  readonly date: string;
  readonly balance: number;
}

/**
 * Builds balance-change points from period snapshots or daily balances.
 */
const buildBalanceChangeChartPoints = function ({
  mode,
  accountingPeriods,
  dates,
}: {
  readonly mode: ChartRangeMode;
  readonly accountingPeriods: readonly PeriodBalanceSummary[];
  readonly dates: readonly DateBalanceSummary[];
}): BarMetricChartPoint[] {
  if (mode === "AccountingPeriod") {
    return accountingPeriods.map(({ name, openingBalance, closingBalance }) => {
      const value = getCurrencyDifference(closingBalance, openingBalance);
      return createAccountingPeriodMetricPoint(
        { name, value },
        getSignedChartColor(value),
      );
    });
  }

  if (dates.length === 1) {
    const [firstDate] = dates;
    return typeof firstDate === "undefined"
      ? []
      : [createDateMetricPoint({ date: firstDate.date, value: 0 }, "neutral")];
  }

  return dates.slice(1).map((summary, index) => {
    const value = getCurrencyDifference(
      summary.balance,
      dates[index]?.balance ?? 0,
    );
    return createDateMetricPoint(
      { date: summary.date, value },
      getSignedChartColor(value),
    );
  });
};

export { buildBalanceChangeChartPoints };
