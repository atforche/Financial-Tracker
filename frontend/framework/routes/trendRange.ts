import propertyName from "@/framework/data/propertyName";

/**
 * Defines the mode of a trend range, which can be based on accounting periods or specific dates.
 */
type TrendRangeMode = "accounting-period" | "date";

/**
 * Default values used when switching a trend range mode.
 */
interface TrendRangeDefaults {
  readonly defaultAccountingPeriodId: string | null;
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
    defaultAccountingPeriodId,
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
  if (defaultAccountingPeriodId !== null) {
    params.set(
      propertyName<TrendRangeSearchParams>("startAccountingPeriodId"),
      params.get(
        propertyName<TrendRangeSearchParams>("startAccountingPeriodId"),
      ) ?? defaultAccountingPeriodId,
    );
    params.set(
      propertyName<TrendRangeSearchParams>("endAccountingPeriodId"),
      params.get(
        propertyName<TrendRangeSearchParams>("endAccountingPeriodId"),
      ) ?? defaultAccountingPeriodId,
    );
  }
};

export { type TrendRangeMode, setTrendRangeMode };
