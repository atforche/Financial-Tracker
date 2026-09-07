import dayjs, { type Dayjs } from "dayjs";
import propertyName from "@/framework/data/propertyName";

const defaultTrendDateRangeDays = 90;
const defaultTrendAccountingPeriodCount = 12;

interface TrendRange {
  readonly start: string;
  readonly end: string;
}

/** Returns the default date range used by trends pages. */
const getDefaultTrendDateRange = function (
  endDate: Dayjs = dayjs(),
): TrendRange {
  return {
    start: endDate
      .subtract(defaultTrendDateRangeDays, "day")
      .format("YYYY-MM-DD"),
    end: endDate.format("YYYY-MM-DD"),
  };
};

/**
 * Returns the default range from accounting periods sorted newest first.
 */
const getDefaultTrendAccountingPeriodRange = function (
  accountingPeriods: readonly { readonly id: string }[],
): TrendRange | null {
  const [end] = accountingPeriods;
  if (typeof end === "undefined") {
    return null;
  }
  const start =
    accountingPeriods[
      Math.min(
        defaultTrendAccountingPeriodCount - 1,
        accountingPeriods.length - 1,
      )
    ] ?? end;
  return { start: start.id, end: end.id };
};

/**
 * Defines the mode of a trend range, which can be based on accounting periods or specific dates.
 */
type TrendRangeMode = "accounting-period" | "date";

/**
 * Default values used when switching a trend range mode.
 */
interface TrendRangeDefaults {
  readonly defaultStartAccountingPeriodId: string | null;
  readonly defaultEndAccountingPeriodId: string | null;
  readonly defaultStartDate: string;
  readonly defaultEndDate: string;
}

/**
 * Search parameters used to configure a trend range.
 */
interface TrendRangeSearchParams {
  mode?: TrendRangeMode;
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
}

/**
 * Switches a trend range mode while retaining values for that mode and clearing dependent parameters.
 */
const setTrendRangeMode = function (
  params: URLSearchParams,
  nextMode: TrendRangeMode,
  {
    defaultStartAccountingPeriodId,
    defaultEndAccountingPeriodId,
    defaultStartDate,
    defaultEndDate,
  }: TrendRangeDefaults,
): void {
  params.set(propertyName<TrendRangeSearchParams>("mode"), nextMode);

  if (nextMode === "date") {
    params.delete(
      propertyName<TrendRangeSearchParams>("startAccountingPeriodId"),
    );
    params.delete(
      propertyName<TrendRangeSearchParams>("endAccountingPeriodId"),
    );
    params.set(
      propertyName<TrendRangeSearchParams>("startDate"),
      params.get(propertyName<TrendRangeSearchParams>("startDate")) ??
        defaultStartDate,
    );
    params.set(
      propertyName<TrendRangeSearchParams>("endDate"),
      params.get(propertyName<TrendRangeSearchParams>("endDate")) ??
        defaultEndDate,
    );
    return;
  }

  params.delete(propertyName<TrendRangeSearchParams>("startDate"));
  params.delete(propertyName<TrendRangeSearchParams>("endDate"));
  if (
    defaultStartAccountingPeriodId !== null &&
    defaultEndAccountingPeriodId !== null
  ) {
    params.set(
      propertyName<TrendRangeSearchParams>("startAccountingPeriodId"),
      params.get(
        propertyName<TrendRangeSearchParams>("startAccountingPeriodId"),
      ) ?? defaultStartAccountingPeriodId,
    );
    params.set(
      propertyName<TrendRangeSearchParams>("endAccountingPeriodId"),
      params.get(
        propertyName<TrendRangeSearchParams>("endAccountingPeriodId"),
      ) ?? defaultEndAccountingPeriodId,
    );
  }
};

export {
  type TrendRangeMode,
  getDefaultTrendAccountingPeriodRange,
  getDefaultTrendDateRange,
  setTrendRangeMode,
};
