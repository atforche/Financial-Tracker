import type { ChartColor, ChartRangeMode } from "@/framework/charts/chartTypes";
import { BalanceGranularityModel } from "@/framework/data/api";
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
 * Formats the tick and tooltip labels for a date that ends a day, week, or month of a series.
 */
const getDateLabels = function (
  date: string,
  granularity: BalanceGranularityModel,
  dayTickFormat: string,
): { readonly tickLabel: string; readonly tooltipLabel: string } {
  const longDate = formatLongDate(new Date(`${date}T00:00:00`));
  if (granularity === BalanceGranularityModel.Week) {
    return {
      tickLabel: dayjs(date).format("MMM D"),
      tooltipLabel: `Week ending ${longDate}`,
    };
  }
  if (granularity === BalanceGranularityModel.Month) {
    return {
      tickLabel: dayjs(date).format("MMM YYYY"),
      tooltipLabel: `Month ending ${longDate}`,
    };
  }
  return {
    tickLabel: dayjs(date).format(dayTickFormat),
    tooltipLabel: longDate,
  };
};

/**
 * Creates a normalized metric point for a date that ends a day, week, or month.
 */
const createDateMetricPoint = function (
  metric: DateMetric,
  color?: ChartColor,
  granularity: BalanceGranularityModel = BalanceGranularityModel.Day,
): BarMetricChartPoint {
  return {
    ...getDateLabels(metric.date, granularity, "MMM D"),
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
  getDateLabels,
};
