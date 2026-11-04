import type { AccountBalanceEventSort, AccountType } from "@/accounts/types";
import type { GoalRangeSearchParams } from "@/framework/goals/resolveGoalRange";
import type { TrendRangeSearchParams } from "@/framework/routes/trendRange";

/**
 * Defines the possible actions that can be performed in the Accounts workspace.
 */
type AccountWorkspaceAction = "create" | "onboard";

/**
 * Search parameters supported by the Accounts workspace.
 */
interface AccountWorkspaceSearchParams
  extends TrendRangeSearchParams, GoalRangeSearchParams {
  tab?: string;
  search?: string;
  accountType?: AccountType | readonly AccountType[];
  action?: AccountWorkspaceAction;
  balanceEventPage?: number | string | null;
  pageSize?: number | string | null;
  balanceEventSort?: AccountBalanceEventSort;
}

export type { AccountWorkspaceAction, AccountWorkspaceSearchParams };
