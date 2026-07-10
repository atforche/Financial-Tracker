import type { CurrentGoalProgress, CurrentGoals } from "@/goals/types";

/**
 * Creates an empty current goals object.
 */
const createEmptyGoals = function (): CurrentGoals {
  return {
    accountingPeriodId: null,
    accountingPeriodName: null,
    availableFundNames: [],
    summary: {
      totalAmountToAssign: 0,
      totalAmountAssigned: 0,
      percentageOfAssignmentGoalsMet: {
        totalCount: 0,
        metCount: 0,
        percentageMet: 0,
      },
      totalAmountToSpend: 0,
      totalAmountSpent: 0,
      percentageOfSpendingGoalsMet: {
        totalCount: 0,
        metCount: 0,
        percentageMet: 0,
      },
    },
    goals: [],
  };
};

/**
 * Gets the goal progress percent for the provided goal progress.
 */
const getGoalProgressPercent = function (
  progress: CurrentGoalProgress | null,
): number {
  if (progress === null) {
    return 0;
  }
  if (progress.targetAmount === 0) {
    return 100;
  }
  return Math.min((progress.currentAmount / progress.targetAmount) * 100, 100);
};

/**
 * Gets the goal progress background color for the provided goal progress and progress percent.
 */
const getGoalProgressBackgroundColor = function (
  progress: CurrentGoalProgress | null,
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
  createEmptyGoals,
  getGoalProgressPercent,
  getGoalProgressBackgroundColor,
};
