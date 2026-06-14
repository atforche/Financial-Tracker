import type {
  AssignmentGoal,
  AssignmentGoalSortOrder,
  AssignmentGoalType,
  GoalDashboardBalanceEvent,
  SpendingGoal,
  SpendingGoalSortOrder,
  SpendingGoalType,
} from "@/goals/types";

type GoalDashboardView = "assignment" | "spending";

type GoalDashboardGoal = AssignmentGoal | SpendingGoal;

type GoalDashboardSortOrder = AssignmentGoalSortOrder | SpendingGoalSortOrder;

type GoalDashboardGoalType = AssignmentGoalType | SpendingGoalType;

const defaultGoalDashboardView: GoalDashboardView = "assignment";

/**
 * Determines whether the provided URL value is a supported goal dashboard view.
 */
const isGoalDashboardView = function (
  value: string | null | undefined,
): value is GoalDashboardView {
  return value === "assignment" || value === "spending";
};

export type {
  GoalDashboardBalanceEvent,
  GoalDashboardGoal,
  GoalDashboardGoalType,
  GoalDashboardSortOrder,
  GoalDashboardView,
};
export { defaultGoalDashboardView, isGoalDashboardView };
