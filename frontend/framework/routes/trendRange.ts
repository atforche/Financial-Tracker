/**
 * Trend range modes supported by date and accounting-period filters.
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
  params.set("mode", nextMode);

  if (nextMode === "date") {
    params.delete("startAccountingPeriodId");
    params.delete("endAccountingPeriodId");
    params.set("startDate", params.get("startDate") ?? defaultStartDate);
    params.set("endDate", params.get("endDate") ?? defaultEndDate);
    return;
  }

  params.delete("startDate");
  params.delete("endDate");
  if (defaultAccountingPeriodId !== null) {
    params.set(
      "startAccountingPeriodId",
      params.get("startAccountingPeriodId") ?? defaultAccountingPeriodId,
    );
    params.set(
      "endAccountingPeriodId",
      params.get("endAccountingPeriodId") ?? defaultAccountingPeriodId,
    );
  }
};

export { type TrendRangeMode, setTrendRangeMode };
