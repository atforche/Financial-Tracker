import {
  AccountGoalEndingBalanceStatus,
  type AccountGoalProgress,
} from "@/account-goals/types";

/**
 * Returns the configured Account Goal dimensions and the dimensions satisfied.
 */
const getAccountGoalDimensionSummary = function (
  progress: AccountGoalProgress,
  maximumConfigured: boolean,
): { configured: number; satisfied: number } {
  const configured = 1 + (maximumConfigured ? 1 : 0);
  const satisfied =
    (progress.endingBalance.status !==
    AccountGoalEndingBalanceStatus.BelowMinimum
      ? 1
      : 0) +
    (maximumConfigured &&
    progress.endingBalance.status !==
      AccountGoalEndingBalanceStatus.AboveMaximum
      ? 1
      : 0);
  return { configured, satisfied };
};

export { getAccountGoalDimensionSummary };
