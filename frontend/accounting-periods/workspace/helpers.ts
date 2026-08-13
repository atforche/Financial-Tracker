import type { AccountingPeriod } from "@/accounting-periods/types";

/**
 * Possible actions that can be performed in the accounting period workspace.
 */
const accountingPeriodWorkspaceActions = [
  "create",
  "close",
  "reopen",
  "editExpectedIncome",
  "delete",
] as const;

/**
 * Type representing the possible actions that can be performed in the Accounting Period workspace.
 */
type AccountingPeriodWorkspaceAction =
  (typeof accountingPeriodWorkspaceActions)[number];

/**
 * Labels for the actions available in the accounting period workspace.
 */
const accountingPeriodWorkspaceActionLabels: Record<
  AccountingPeriodWorkspaceAction,
  string
> = {
  create: "Create",
  close: "Close",
  reopen: "Reopen",
  editExpectedIncome: "Edit Expected Income",
  delete: "Delete",
};

/**
 * Gets the actions available for the current accounting period selection.
 */
const getAvailableAccountingPeriodWorkspaceActions = function (
  accountingPeriod: AccountingPeriod | null,
): readonly AccountingPeriodWorkspaceAction[] {
  if (accountingPeriod === null) {
    return ["create"];
  }
  return accountingPeriod.isOpen
    ? ["close", "editExpectedIncome", "delete", "create"]
    : ["reopen", "delete", "create"];
};

/**
 * Type representing the possible actions that can be performed in the accounting period workspace.
 */
export type { AccountingPeriodWorkspaceAction };
export {
  accountingPeriodWorkspaceActions,
  accountingPeriodWorkspaceActionLabels,
  getAvailableAccountingPeriodWorkspaceActions,
};
