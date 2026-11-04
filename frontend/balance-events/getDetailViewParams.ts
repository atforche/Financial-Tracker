import type { GoalRangeSearchParams } from "@/framework/goals/resolveGoalRange";
import type { TrendRangeSearchParams } from "@/framework/routes/trendRange";

/**
 * Search parameters choosing what an account or fund page shows.
 */
type DetailViewParams = TrendRangeSearchParams &
  GoalRangeSearchParams & { tab?: string };

const detailViewParamNames = [
  "tab",
  "mode",
  "startDate",
  "endDate",
  "startAccountingPeriodId",
  "endAccountingPeriodId",
  "goalStartPeriodId",
  "goalEndPeriodId",
] as const;

/**
 * Keeps the selected tab and ranges, so saving a goal returns to the same view.
 */
const getDetailViewParams = function (
  params: DetailViewParams,
): DetailViewParams {
  return Object.fromEntries(
    detailViewParamNames.flatMap((name) =>
      params[name] === undefined ? [] : [[name, params[name]]],
    ),
  );
};

export default getDetailViewParams;
