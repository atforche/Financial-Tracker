import type { ChartColor, ChartRangeMode } from "@/framework/charts/chartTypes";
import type { BarMetricChartPoint } from "@/framework/charts/barMetricHelpers";
import dayjs from "dayjs";
import { formatLongDate } from "@/framework/dateHelpers";

/**
 * Type representing a metric for an accounting period.
 */
interface AccountingPeriodMetric {
  readonly name: string;
  readonly value: number;
}

/**
 * Type representing a metric for a calendar date.
 */
interface DateMetric {
  readonly date: string;
  readonly value: number;
}

/**
 * Creates a normalized metric point for an accounting period.
 */
const createAccountingPeriodMetricPoint = function (
  metric: AccountingPeriodMetric,
  color?: ChartColor,
): BarMetricChartPoint {
  return {
    tickLabel: metric.name,
    tooltipLabel: metric.name,
    value: metric.value,
    ...(typeof color === "undefined" ? {} : { color }),
  };
};

/**
 * Creates a normalized metric point for a calendar date.
 */
const createDateMetricPoint = function (
  metric: DateMetric,
  color?: ChartColor,
): BarMetricChartPoint {
  return {
    tickLabel: dayjs(metric.date).format("MMM D"),
    tooltipLabel: formatLongDate(new Date(`${metric.date}T00:00:00`)),
    value: metric.value,
    ...(typeof color === "undefined" ? {} : { color }),
  };
};

/**
 * Builds normalized metric points for the active trends grouping.
 */
const buildRangeMetricChartPoints = function ({
  mode,
  accountingPeriods,
  dates,
  getColor,
}: {
  readonly mode: ChartRangeMode;
  readonly accountingPeriods: readonly AccountingPeriodMetric[];
  readonly dates: readonly DateMetric[];
  readonly getColor?: (value: number) => ChartColor;
}): BarMetricChartPoint[] {
  if (mode === "AccountingPeriod") {
    return accountingPeriods.map((metric) =>
      createAccountingPeriodMetricPoint(metric, getColor?.(metric.value)),
    );
  }

  return dates.map((metric) =>
    createDateMetricPoint(metric, getColor?.(metric.value)),
  );
};

export {
  buildRangeMetricChartPoints,
  createAccountingPeriodMetricPoint,
  createDateMetricPoint,
};
