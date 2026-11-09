import dayjs, { type Dayjs } from "dayjs";
import type { ToggleButtonSelectorOption } from "@/framework/forms/ToggleButtonSelector";
import propertyName from "@/framework/data/propertyName";

const defaultTrendDateRangeDays = 90;
const defaultTrendAccountingPeriodCount = 12;
// Matches the longest date range the backend will summarize into a balance series
const maximumTrendRangeYears = 10;

/**
 * The start and end of a trend range.
 */
interface TrendRange {
  readonly start: string;
  readonly end: string;
}

/**
 * Returns the default date range used by trends pages.
 */
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
  periodCount: number = defaultTrendAccountingPeriodCount,
): TrendRange | null {
  const [end] = accountingPeriods;
  if (typeof end === "undefined") {
    return null;
  }
  const start =
    accountingPeriods[
      Math.min(periodCount - 1, accountingPeriods.length - 1)
    ] ?? end;
  return { start: start.id, end: end.id };
};

/**
 * Defines the mode of a trend range, which can be based on accounting periods or specific dates.
 */
type TrendRangeMode = "accounting-period" | "date";

/**
 * Returns the options for choosing a trend range mode, explaining why accounting periods are unavailable.
 */
const getTrendRangeModeOptions = function (
  isAccountingPeriodModeAvailable: boolean,
): ToggleButtonSelectorOption<TrendRangeMode>[] {
  return [
    { value: "date", label: "Dates" },
    {
      value: "accounting-period",
      label: "Accounting periods",
      disabled: !isAccountingPeriodModeAvailable,
      disabledReason: "Create an accounting period to view by period",
    },
  ];
};

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
 * A validated trend range in the shape of its search parameters.
 */
type ResolvedTrendRange =
  | {
      readonly mode: "date";
      readonly startDate: string;
      readonly endDate: string;
    }
  | {
      readonly mode: "accounting-period";
      readonly startAccountingPeriodId: string;
      readonly endAccountingPeriodId: string;
    };

/**
 * Determines whether a value is a real calendar date in YYYY-MM-DD format.
 */
const isIsoDate = function (value: string | undefined): value is string {
  return (
    typeof value === "string" && dayjs(value).format("YYYY-MM-DD") === value
  );
};

/**
 * Resolves trend range search parameters, replacing invalid or unknown values with defaults, ordering the range, and
 * shortening date ranges longer than the maximum.
 */
const resolveTrendRange = function (
  params: TrendRangeSearchParams,
  accountingPeriods: readonly { readonly id: string }[],
): ResolvedTrendRange {
  const defaultAccountingPeriodRange =
    getDefaultTrendAccountingPeriodRange(accountingPeriods);
  if (
    params.mode === "accounting-period" &&
    defaultAccountingPeriodRange !== null
  ) {
    const resolvePeriod = (
      id: string | undefined,
      fallback: string,
    ): { id: string; index: number } => {
      const known = accountingPeriods.findIndex((period) => period.id === id);
      const index =
        known === -1
          ? accountingPeriods.findIndex((period) => period.id === fallback)
          : known;
      return { id: accountingPeriods[index]?.id ?? fallback, index };
    };
    const { id: startAccountingPeriodId, index: startIndex } = resolvePeriod(
      params.startAccountingPeriodId,
      defaultAccountingPeriodRange.start,
    );
    const { id: endAccountingPeriodId, index: endIndex } = resolvePeriod(
      params.endAccountingPeriodId,
      defaultAccountingPeriodRange.end,
    );
    // Accounting periods are sorted newest first, so the start has the larger index
    return startIndex >= endIndex
      ? {
          mode: "accounting-period",
          startAccountingPeriodId,
          endAccountingPeriodId,
        }
      : {
          mode: "accounting-period",
          startAccountingPeriodId: endAccountingPeriodId,
          endAccountingPeriodId: startAccountingPeriodId,
        };
  }

  const defaultDateRange = getDefaultTrendDateRange();
  const startDate = isIsoDate(params.startDate)
    ? params.startDate
    : defaultDateRange.start;
  const endDate = isIsoDate(params.endDate)
    ? params.endDate
    : defaultDateRange.end;
  const [orderedStart, orderedEnd] =
    startDate <= endDate ? [startDate, endDate] : [endDate, startDate];
  const earliestStart = dayjs(orderedEnd)
    .subtract(maximumTrendRangeYears, "year")
    .format("YYYY-MM-DD");
  return {
    mode: "date",
    startDate: orderedStart < earliestStart ? earliestStart : orderedStart,
    endDate: orderedEnd,
  };
};

/**
 * Determines whether search parameters already describe the resolved trend range exactly.
 */
const matchesTrendRange = function (
  params: TrendRangeSearchParams,
  range: ResolvedTrendRange,
): boolean {
  if (params.mode !== range.mode) {
    return false;
  }
  return range.mode === "date"
    ? params.startDate === range.startDate && params.endDate === range.endDate
    : params.startAccountingPeriodId === range.startAccountingPeriodId &&
        params.endAccountingPeriodId === range.endAccountingPeriodId;
};

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

/**
 * Gets the IDs of the periods from one period to another, inclusive, in the given list's order.
 */
const getAccountingPeriodIdsInRange = function (
  accountingPeriods: readonly { readonly id: string }[],
  startAccountingPeriodId: string,
  endAccountingPeriodId: string,
): string[] {
  const ids = accountingPeriods.map((period) => period.id);
  const start = ids.indexOf(startAccountingPeriodId);
  const end = ids.indexOf(endAccountingPeriodId);
  return ids.slice(Math.min(start, end), Math.max(start, end) + 1);
};

export {
  type TrendRangeMode,
  getAccountingPeriodIdsInRange,
  getDefaultTrendAccountingPeriodRange,
  getDefaultTrendDateRange,
  getTrendRangeModeOptions,
  matchesTrendRange,
  maximumTrendRangeYears,
  resolveTrendRange,
  setTrendRangeMode,
};
