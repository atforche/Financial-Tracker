import type { AccountType } from "@/accounts/types";

/**
 * Defines the possible actions that can be performed in the Accounts workspace.
 */
type AccountWorkspaceAction = "create" | "onboard";

/**
 * Search parameters supported by the Accounts workspace.
 */
interface AccountWorkspaceSearchParams {
  search?: string;
  accountType?: AccountType | readonly AccountType[];
  action?: AccountWorkspaceAction;
  balanceEventPage?: number | string | null;
}

export type { AccountWorkspaceAction, AccountWorkspaceSearchParams };
