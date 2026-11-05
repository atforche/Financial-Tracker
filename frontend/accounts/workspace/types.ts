import type { AccountType } from "@/accounts/types";
import type { DetailViewParams } from "@/balance-events/getDetailViewParams";

/**
 * Defines the possible actions that can be performed in the Accounts workspace.
 */
type AccountWorkspaceAction = "create" | "onboard";

/**
 * Search parameters supported by the Accounts workspace.
 */
interface AccountWorkspaceSearchParams extends DetailViewParams {
  search?: string;
  accountType?: AccountType | readonly AccountType[];
  action?: AccountWorkspaceAction;
}

export type { AccountWorkspaceAction, AccountWorkspaceSearchParams };
