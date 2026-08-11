import type { FundBalanceEventSort } from "@/funds/types";

/**
 * Search parameters supported by the Funds workspace.
 */
interface FundWorkspaceSearchParams {
  search?: string;
  balanceEventPage?: number | string | null;
  pageSize?: number | string | null;
  balanceEventSort?: FundBalanceEventSort;
}

export type { FundWorkspaceSearchParams };
