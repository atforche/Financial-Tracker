import type { DetailViewParams } from "@/balance-events/getDetailViewParams";

/**
 * Search parameters supported by the Funds workspace.
 */
interface FundWorkspaceSearchParams extends DetailViewParams {
  search?: string;
}

export type { FundWorkspaceSearchParams };
