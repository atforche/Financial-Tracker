import {
  AccountGoalEndingBalanceStatusModel,
  AccountGoalSortModel,
  type components,
} from "@/framework/data/api";

/**
 * Type representing an Account Goal.
 */
type AccountGoal = components["schemas"]["AccountGoalModel"];

/**
 * Type representing Account Goal progress.
 */
type AccountGoalProgress = components["schemas"]["AccountGoalProgressModel"];

/**
 * An Account Goal paired with its calculated progress.
 */
interface AccountGoalWithProgress extends AccountGoal {
  readonly progress: AccountGoalProgress;
}

/**
 * Type representing a request to update an Account Goal.
 */
type UpdateAccountGoalRequest = components["schemas"]["UpdateAccountGoalModel"];

export {
  type AccountGoal,
  AccountGoalEndingBalanceStatusModel as AccountGoalEndingBalanceStatus,
  type AccountGoalProgress,
  type AccountGoalWithProgress,
  AccountGoalSortModel as AccountGoalSort,
  type UpdateAccountGoalRequest,
};
