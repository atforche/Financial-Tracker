/**
 * Search parameters supported by the Account Goal workspace.
 */
interface AccountGoalWorkspaceSearchParams {
  accountingPeriodId?: string;
  accountIds?: string | readonly string[];
  returnUrl?: string;
}

export type { AccountGoalWorkspaceSearchParams };
