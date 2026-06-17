import type {
  AssignmentGoal,
  AssignmentGoalSortOrder,
  AssignmentGoalType,
  GoalTrendsBalanceEvent,
  SpendingGoal,
  SpendingGoalSortOrder,
  SpendingGoalType,
} from "@/goals/types";

type GoalTrendsView = "assignment" | "spending";

type GoalTrendsGoal = AssignmentGoal | SpendingGoal;

type GoalTrendsSortOrder = AssignmentGoalSortOrder | SpendingGoalSortOrder;

type GoalTrendsGoalType = AssignmentGoalType | SpendingGoalType;

const defaultGoalTrendsView: GoalTrendsView = "assignment";

/**
 * Determines whether the provided URL value is a supported goal trends view.
 */
const isGoalTrendsView = function (
  value: string | null | undefined,
): value is GoalTrendsView {
  return value === "assignment" || value === "spending";
};

export type {
  GoalTrendsBalanceEvent,
  GoalTrendsGoal,
  GoalTrendsGoalType,
  GoalTrendsSortOrder,
  GoalTrendsView,
};
export { defaultGoalTrendsView, isGoalTrendsView };
