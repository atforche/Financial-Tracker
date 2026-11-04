import type { components } from "@/framework/data/api";

/**
 * Type representing an Account Goal.
 */
type AccountGoal = components["schemas"]["AccountGoalModel"];

/**
 * Type representing Account Goal progress.
 */
type AccountGoalProgress = components["schemas"]["AccountGoalProgressModel"];

/**
 * Type representing a request to update an Account Goal.
 */
type UpdateAccountGoalRequest = components["schemas"]["UpdateAccountGoalModel"];

export {
  type AccountGoal,
  type AccountGoalProgress,
  type UpdateAccountGoalRequest,
};
