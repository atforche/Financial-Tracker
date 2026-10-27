import {
  findTooltipPayload,
  isObject,
} from "@/framework/charts/tooltipHelpers";
import { BalanceGranularityModel } from "@/framework/data/api";
import type { ChartRangeMode } from "@/framework/charts/chartTypes";
import type { TooltipContentProps } from "recharts";
import dayjs from "dayjs";
import { getDateLabels } from "@/framework/charts/chartPointHelpers";

/**
 * Indicates whether the chart is grouped by accounting period or date.
 */
type BalanceTrendChartMode = ChartRangeMode;

/**
 * A balance summary for one day in a balance trend.
 */
interface BalanceTrendDateSummary {
  readonly date: string;
  readonly totalBalance: number;
}

/**
 * A balance summary for one accounting period in a balance trend.
 */
interface BalanceTrendPeriodSummary {
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly year: number;
  readonly month: number;
  readonly totalOpeningBalance: number;
  readonly totalClosingBalance: number;
}

/**
 * A normalized data point rendered by the balance trend chart.
 */
interface BalanceTrendChartPoint {
  readonly tickLabel: string;
  readonly tooltipLabel: string;
  readonly balance: number;
  readonly description?: string;
}

/**
 * The unvalidated shape of a balance trend chart point.
 */
interface BalanceTrendChartPointCandidate {
  readonly tickLabel?: unknown;
  readonly tooltipLabel?: unknown;
  readonly balance?: unknown;
  readonly description?: unknown;
}

/**
 * Determines whether a value has the normalized balance trend chart point shape.
 */
const isBalanceTrendChartPoint = function (
  value: unknown,
): value is BalanceTrendChartPoint {
  if (!isObject(value)) {
    return false;
  }

  const candidate: BalanceTrendChartPointCandidate = {
    tickLabel: value["tickLabel"],
    tooltipLabel: value["tooltipLabel"],
    balance: value["balance"],
    description: value["description"],
  };

  return (
    typeof candidate.tickLabel === "string" &&
    typeof candidate.tooltipLabel === "string" &&
    typeof candidate.balance === "number"
  );
};

/**
 * Converts an unknown tooltip payload value into a normalized chart point.
 */
const toBalanceTrendChartPoint = function (
  value: unknown,
): BalanceTrendChartPoint | null {
  if (!isBalanceTrendChartPoint(value)) {
    return null;
  }

  return {
    tickLabel: value.tickLabel,
    tooltipLabel: value.tooltipLabel,
    balance: value.balance,
    ...(typeof value.description === "string"
      ? { description: value.description }
      : {}),
  };
};

/**
 * Retrieves the first normalized chart point from a Recharts tooltip payload.
 */
const getBalanceTrendTooltipPoint = function (
  tooltipProps: TooltipContentProps,
): BalanceTrendChartPoint | null {
  const point = findTooltipPayload(tooltipProps, isBalanceTrendChartPoint);
  return point === null ? null : toBalanceTrendChartPoint(point);
};

/**
 * Builds normalized chart points from date-based balance summaries.
 */
const buildDateChartPoints = function (
  dates: readonly BalanceTrendDateSummary[],
  granularity: BalanceGranularityModel = BalanceGranularityModel.Day,
): BalanceTrendChartPoint[] {
  return dates.map((dateSummary) => ({
    ...getDateLabels(dateSummary.date, granularity, "MMMM D"),
    balance: dateSummary.totalBalance,
  }));
};

/**
 * Builds normalized chart points from accounting-period balance summaries.
 */
const buildAccountingPeriodChartPoints = function (
  accountingPeriods: readonly BalanceTrendPeriodSummary[],
): BalanceTrendChartPoint[] {
  const openingPoints = accountingPeriods.map((accountingPeriod) => ({
    tickLabel: dayjs(
      new Date(accountingPeriod.year, accountingPeriod.month - 1, 1),
    ).format("MMMM YYYY"),
    tooltipLabel: `${accountingPeriod.accountingPeriodName} opening balance`,
    balance: accountingPeriod.totalOpeningBalance,
  }));

  const lastAccountingPeriod = accountingPeriods.at(-1);
  if (typeof lastAccountingPeriod === "undefined") {
    return openingPoints;
  }

  return [
    ...openingPoints,
    {
      tickLabel: "End",
      tooltipLabel: `${lastAccountingPeriod.accountingPeriodName} closing balance`,
      balance: lastAccountingPeriod.totalClosingBalance,
    },
  ];
};

/**
 * Builds normalized chart points for the selected balance trend grouping.
 */
const buildBalanceTrendChartPoints = function ({
  mode,
  accountingPeriods,
  dates,
  granularity,
}: {
  readonly mode: BalanceTrendChartMode;
  readonly accountingPeriods: readonly BalanceTrendPeriodSummary[] | null;
  readonly dates: readonly BalanceTrendDateSummary[] | null;
  readonly granularity?: BalanceGranularityModel;
}): BalanceTrendChartPoint[] {
  if (mode === "Date") {
    return buildDateChartPoints(dates ?? [], granularity);
  }

  return buildAccountingPeriodChartPoints(accountingPeriods ?? []);
};

export {
  buildBalanceTrendChartPoints,
  buildDateChartPoints,
  getBalanceTrendTooltipPoint,
};
export type { BalanceTrendChartPoint, BalanceTrendDateSummary };
