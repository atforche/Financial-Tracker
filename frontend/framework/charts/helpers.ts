import type { TooltipContentProps } from "recharts";
import dayjs from "dayjs";

/**
 * Indicates whether the chart is grouped by accounting period or date.
 */
type BalanceTrendChartMode = "AccountingPeriod" | "Date";

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
  readonly key: string;
  readonly tickLabel: string;
  readonly tooltipLabel: string;
  readonly balance: number;
}

/**
 * The unvalidated shape of a balance trend chart point.
 */
interface BalanceTrendChartPointCandidate {
  readonly key?: unknown;
  readonly tickLabel?: unknown;
  readonly tooltipLabel?: unknown;
  readonly balance?: unknown;
}

/**
 * Formats y-axis balance values with compact US currency notation.
 */
const compactCurrencyFormatter = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  notation: "compact",
  maximumFractionDigits: 1,
});

/**
 * Determines whether a value is a non-null object.
 */
const isObject = function (
  value: unknown,
): value is Record<PropertyKey, unknown> {
  return typeof value === "object" && value !== null;
};

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
    key: value["key"],
    tickLabel: value["tickLabel"],
    tooltipLabel: value["tooltipLabel"],
    balance: value["balance"],
  };

  return (
    typeof candidate.key === "string" &&
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
    key: value.key,
    tickLabel: value.tickLabel,
    tooltipLabel: value.tooltipLabel,
    balance: value.balance,
  };
};

/**
 * Retrieves the first normalized chart point from a Recharts tooltip payload.
 */
const getTooltipChartPoint = function (
  tooltipProps: TooltipContentProps,
): BalanceTrendChartPoint | null {
  for (const payloadEntry of tooltipProps.payload) {
    const chartPoint = toBalanceTrendChartPoint(payloadEntry.payload);
    if (chartPoint !== null) {
      return chartPoint;
    }
  }

  return null;
};

/**
 * Builds normalized chart points from date-based balance summaries.
 */
const buildDateChartPoints = function (
  dates: readonly BalanceTrendDateSummary[],
): BalanceTrendChartPoint[] {
  return dates.map((dateSummary) => ({
    key: dateSummary.date,
    tickLabel: dayjs(dateSummary.date).format("MMMM D"),
    tooltipLabel: dayjs(dateSummary.date).format("MMMM D, YYYY"),
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
    key: `${accountingPeriod.accountingPeriodId}-opening`,
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
      key: `${lastAccountingPeriod.accountingPeriodId}-closing`,
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
}: {
  readonly mode: BalanceTrendChartMode;
  readonly accountingPeriods: readonly BalanceTrendPeriodSummary[] | null;
  readonly dates: readonly BalanceTrendDateSummary[] | null;
}): BalanceTrendChartPoint[] {
  if (mode === "Date") {
    return buildDateChartPoints(dates ?? []);
  }

  return buildAccountingPeriodChartPoints(accountingPeriods ?? []);
};

export {
  buildBalanceTrendChartPoints,
  compactCurrencyFormatter,
  getTooltipChartPoint,
};
export type {
  BalanceTrendChartMode,
  BalanceTrendChartPoint,
  BalanceTrendDateSummary,
  BalanceTrendPeriodSummary,
};
