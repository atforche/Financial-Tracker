import nameof from "@/framework/data/nameof";

type TrendRangeMode = "accounting-period" | "date";

/**
 * Default values used when switching a trend range mode.
 */
interface TrendRangeDefaults {
  readonly defaultAccountingPeriodId: string | null;
  readonly defaultStartDate: string;
  readonly defaultEndDate: string;
}

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
  params.set(nameof<TrendRangeSearchParams>("mode"), nextMode);

  if (nextMode === "date") {
    params.delete(nameof<TrendRangeSearchParams>("startAccountingPeriodId"));
    params.delete(nameof<TrendRangeSearchParams>("endAccountingPeriodId"));
    params.set(
      nameof<TrendRangeSearchParams>("startDate"),
      params.get(nameof<TrendRangeSearchParams>("startDate")) ??
        defaultStartDate,
    );
    params.set(
      nameof<TrendRangeSearchParams>("endDate"),
      params.get(nameof<TrendRangeSearchParams>("endDate")) ?? defaultEndDate,
    );
    return;
  }

  params.delete(nameof<TrendRangeSearchParams>("startDate"));
  params.delete(nameof<TrendRangeSearchParams>("endDate"));
  if (defaultAccountingPeriodId !== null) {
    params.set(
      nameof<TrendRangeSearchParams>("startAccountingPeriodId"),
      params.get(nameof<TrendRangeSearchParams>("startAccountingPeriodId")) ??
        defaultAccountingPeriodId,
    );
    params.set(
      nameof<TrendRangeSearchParams>("endAccountingPeriodId"),
      params.get(nameof<TrendRangeSearchParams>("endAccountingPeriodId")) ??
        defaultAccountingPeriodId,
    );
  }
};

export { type TrendRangeMode, setTrendRangeMode };
