import type { AccountBalanceEventSort } from "@/accounts/types";

/**
 * Search parameters supported by the Account Goal workspace.
 */
interface AccountGoalWorkspaceSearchParams {
  accountingPeriodId?: string;
  accountIds?: string | readonly string[];
  returnUrl?: string;
  balanceEventPage?: string;
  balanceEventSort?: AccountBalanceEventSort;
  pageSize?: string;
}

export type { AccountGoalWorkspaceSearchParams };
