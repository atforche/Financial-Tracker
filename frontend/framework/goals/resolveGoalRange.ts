import type { AccountingPeriod } from "@/accounting-periods/types";
import { getDefaultTrendAccountingPeriodRange } from "@/framework/routes/trendRange";

/**
 * Search parameters choosing the accounting periods shown by a goal tab.
 */
interface GoalRangeSearchParams {
  goalStartPeriodId?: string;
  goalEndPeriodId?: string;
}

/**
 * Resolves a goal tab's range, or null before any accounting period exists. Unknown periods fall back to the default.
 */
const resolveGoalRange = function (
  params: GoalRangeSearchParams,
  accountingPeriods: readonly AccountingPeriod[],
): { readonly start: string; readonly end: string } | null {
  const fallback = getDefaultTrendAccountingPeriodRange(accountingPeriods);
  const isKnown = (id: string | undefined): id is string =>
    accountingPeriods.some((period) => period.id === id);
  return fallback === null
    ? null
    : {
        start: isKnown(params.goalStartPeriodId)
          ? params.goalStartPeriodId
          : fallback.start,
        end: isKnown(params.goalEndPeriodId)
          ? params.goalEndPeriodId
          : fallback.end,
      };
};

export type { GoalRangeSearchParams };
export default resolveGoalRange;
