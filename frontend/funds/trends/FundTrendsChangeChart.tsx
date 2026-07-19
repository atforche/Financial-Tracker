"use client";

import {
  type BarMetricChartPoint,
  getSignedBarColor,
} from "@/framework/charts/barMetricHelpers";
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
import dayjs from "dayjs";
import { formatLongDate } from "@/framework/dateHelpers";

type FundTrendsChangeChartMode = "AccountingPeriod" | "Date";

interface FundTrendsChangeChartProps {
  readonly mode: FundTrendsChangeChartMode;
  readonly accountingPeriods: readonly FundBalanceSummaryByPeriod[] | null;
  readonly dates: readonly FundBalanceSummaryByDate[] | null;
}

const buildChartPoints = function (
  mode: FundTrendsChangeChartMode,
  accountingPeriods: readonly FundBalanceSummaryByPeriod[],
  dates: readonly FundBalanceSummaryByDate[],
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
      : [
          {
            key: summary.date,
            tickLabel: dayjs(summary.date).format("MMM D"),
            tooltipLabel: formatLongDate(new Date(`${summary.date}T00:00:00`)),
            value: 0,
            fill: getSignedBarColor(0),
          },
        ];
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
      chartPoints={buildChartPoints(mode, accountingPeriods ?? [], dates ?? [])}
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

export default FundTrendsChangeChart;
