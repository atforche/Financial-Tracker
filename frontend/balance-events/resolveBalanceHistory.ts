import {
  type ResolvedTrendRange,
  type TrendRangeSearchParams,
  resolveTrendRange,
} from "@/framework/routes/trendRange";
import type { AccountingPeriod } from "@/accounting-periods/types";
import dayjs from "dayjs";

/**
 * The selected range of an account or fund's history, with the calendar dates it covers.
 */
type BalanceHistoryRange = ResolvedTrendRange & {
  readonly rangeStart: string;
  readonly rangeEnd: string;
};

/**
 * Resolves a history range from its search parameters. Balance events are queried by date, so an accounting period
 * range is converted to the calendar months it covers. Accounting periods are sorted newest first.
 */
const resolveBalanceHistory = function (
  params: TrendRangeSearchParams,
  accountingPeriods: readonly AccountingPeriod[],
): BalanceHistoryRange {
  const range = resolveTrendRange(params, accountingPeriods);
  if (range.mode === "date") {
    return { ...range, rangeStart: range.startDate, rangeEnd: range.endDate };
  }
  const getMonth = function (id: string): dayjs.Dayjs {
    const period = accountingPeriods.find((item) => item.id === id);
    return dayjs(new Date(period?.year ?? 0, (period?.month ?? 1) - 1, 1));
  };
  return {
    ...range,
    rangeStart: getMonth(range.startAccountingPeriodId).format("YYYY-MM-DD"),
    rangeEnd: getMonth(range.endAccountingPeriodId)
      .endOf("month")
      .format("YYYY-MM-DD"),
  };
};

export type { BalanceHistoryRange };
export default resolveBalanceHistory;
