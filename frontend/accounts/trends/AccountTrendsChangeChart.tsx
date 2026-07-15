"use client";

import type {
  AccountBalanceSummaryByDate,
  AccountBalanceSummaryByPeriod,
} from "@/accounts/types";
import {
  type BarMetricChartPoint,
  formatSignedCurrency,
  getSignedBarColor,
} from "@/framework/charts/barMetricHelpers";
import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { JSX } from "react";
import dayjs from "dayjs";
import formatCompactCurrency from "@/framework/formatCompactCurrency";
import formatLongDate from "@/framework/formatLongDate";

type AccountTrendsChangeChartMode = "AccountingPeriod" | "Date";

interface AccountTrendsChangeChartProps {
  readonly mode: AccountTrendsChangeChartMode;
  readonly accountingPeriods: readonly AccountBalanceSummaryByPeriod[] | null;
  readonly dates: readonly AccountBalanceSummaryByDate[] | null;
}

const buildChartPoints = function (
  mode: AccountTrendsChangeChartMode,
  accountingPeriods: readonly AccountBalanceSummaryByPeriod[],
  dates: readonly AccountBalanceSummaryByDate[],
): BarMetricChartPoint[] {
  if (mode === "AccountingPeriod") {
    return accountingPeriods.map((summary) => {
      const value =
        summary.closingBalance.totalBalance -
        summary.openingBalance.totalBalance;
      return {
        key: summary.accountingPeriod.id,
        tickLabel: summary.accountingPeriod.name,
        tooltipLabel: summary.accountingPeriod.name,
        value,
        fill: getSignedBarColor(value),
      };
    });
  }

  if (dates.length === 1) {
    const [summary] = dates;
    return typeof summary === "undefined"
      ? []
      : [{
          key: summary.date,
          tickLabel: dayjs(summary.date).format("MMM D"),
          tooltipLabel: formatLongDate(new Date(`${summary.date}T00:00:00`)),
          value: 0,
          fill: getSignedBarColor(0),
        }];
  }

  return dates.slice(1).map((summary, index) => {
    const value = summary.totalBalance - (dates[index]?.totalBalance ?? 0);
    return {
      key: summary.date,
      tickLabel: dayjs(summary.date).format("MMM D"),
      tooltipLabel: formatLongDate(new Date(`${summary.date}T00:00:00`)),
      value,
      fill: getSignedBarColor(value),
    };
  });
};

/** Renders balance changes for the account trends. */
const AccountTrendsChangeChart = function ({
  mode,
  accountingPeriods,
  dates,
}: AccountTrendsChangeChartProps): JSX.Element {
  return (
    <BarMetricChart
      title="Balance Change"
      emptyMessage="No balance changes are available for the selected trends range."
      chartPoints={buildChartPoints(
        mode,
        accountingPeriods ?? [],
        dates ?? [],
      )}
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
