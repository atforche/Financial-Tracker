import type { AccountType } from "@/accounts/types";
import type { DetailViewParams } from "@/balance-events/getDetailViewParams";

/**
 * Search parameters supported by the Accounts workspace.
 */
interface AccountWorkspaceSearchParams extends DetailViewParams {
  search?: string;
  accountType?: AccountType | readonly AccountType[];
  institution?: string;
}

export type { AccountWorkspaceSearchParams };
