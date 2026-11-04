import type { FundBalanceEventSort } from "@/funds/types";
import type { GoalRangeSearchParams } from "@/framework/goals/resolveGoalRange";
import type { TrendRangeSearchParams } from "@/framework/routes/trendRange";

/**
 * Search parameters supported by the Funds workspace.
 */
interface FundWorkspaceSearchParams
  extends TrendRangeSearchParams, GoalRangeSearchParams {
  tab?: string;
  search?: string;
  balanceEventPage?: number | string | null;
  pageSize?: number | string | null;
  balanceEventSort?: FundBalanceEventSort;
}

export type { FundWorkspaceSearchParams };
