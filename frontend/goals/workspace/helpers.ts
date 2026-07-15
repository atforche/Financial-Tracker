import type { AssignmentGoal, SpendingGoal } from "@/goals/types";

type GoalProgress = AssignmentGoal | SpendingGoal;

/**
 * Gets the amounts used to display progress for either kind of Goal.
 */
const getGoalProgressAmounts = function (progress: GoalProgress): {
  current: number;
  remaining: number;
  target: number;
} {
  if ("goalAmount" in progress) {
    return {
      current: progress.totalAmountAssigned,
      remaining: progress.remainingAmountToAssign,
      target: progress.totalAmountToAssign,
    };
  }
  return {
    current: progress.totalAmountSpent,
    remaining: progress.remainingAmountToSpend,
    target: progress.totalAmountToSpend,
  };
};

/**
 * Gets the goal progress percent for the provided goal progress.
 */
const getGoalProgressPercent = function (
  progress: GoalProgress | null,
): number {
  if (progress === null) {
    return 0;
  }
  const amounts = getGoalProgressAmounts(progress);
  if (amounts.target === 0) {
    return 100;
  }
  return Math.min((amounts.current / amounts.target) * 100, 100);
};

/**
 * Gets the goal progress background color for the provided goal progress and progress percent.
 */
const getGoalProgressBackgroundColor = function (
  progress: GoalProgress | null,
  progressPercent: number,
): string {
  if (progress?.isGoalMet === true) {
    return "success.main";
  }
  if (progressPercent === 100) {
    return "error.main";
  }
  return "primary.main";
};

export {
  getGoalProgressAmounts,
  getGoalProgressPercent,
  getGoalProgressBackgroundColor,
};
