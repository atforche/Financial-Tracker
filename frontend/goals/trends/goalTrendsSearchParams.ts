import {
  type GoalTrendsView,
  defaultGoalTrendsView,
} from "@/goals/trends/goalTrendsTypes";
import type { GoalTrendsSearchParams } from "@/goals/trends/GoalTrends";
import propertyName from "@/framework/data/propertyName";

/**
 * Defines the search parameter names for the GoalTrends component, ensuring type safety and consistency across the application.
 */
const goalTrendsParamNames = {
  sort: propertyName<GoalTrendsSearchParams>("sort"),
  page: propertyName<GoalTrendsSearchParams>("page"),
  balanceEventSort: propertyName<GoalTrendsSearchParams>("balanceEventSort"),
  balanceEventPage: propertyName<GoalTrendsSearchParams>("balanceEventPage"),
  view: propertyName<GoalTrendsSearchParams>("view"),
  goalType: propertyName<GoalTrendsSearchParams>("goalType"),
  fundName: propertyName<GoalTrendsSearchParams>("fundName"),
  startAccountingPeriodId: propertyName<GoalTrendsSearchParams>(
    "startAccountingPeriodId",
  ),
  endAccountingPeriodId: propertyName<GoalTrendsSearchParams>(
    "endAccountingPeriodId",
  ),
} as const;

/**
 * Determines whether any user-selectable trends filters are active.
 */
const hasGoalTrendsFilters = function (params: URLSearchParams): boolean {
  return (
    params.getAll(goalTrendsParamNames.goalType).length > 0 ||
    params.getAll(goalTrendsParamNames.fundName).length > 0 ||
    params.has(goalTrendsParamNames.startAccountingPeriodId) ||
    params.has(goalTrendsParamNames.endAccountingPeriodId)
  );
};

/**
 * Clears trends parameters while preserving a non-default view.
 */
const resetGoalTrendsParams = function (
  params: URLSearchParams,
  view: GoalTrendsView,
): void {
  [...params.keys()].forEach((key) => {
    params.delete(key);
  });
  if (view !== defaultGoalTrendsView) {
    params.set(goalTrendsParamNames.view, view);
  }
};

export { goalTrendsParamNames, hasGoalTrendsFilters, resetGoalTrendsParams };
