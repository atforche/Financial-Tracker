/**
 * Possible actions that can be performed in the Accounting Period workspace.
 */
const accountingPeriodWorkspaceActions = [
  "create",
  "close",
  "reopen",
  "delete",
] as const;

/**
 * Type representing the possible actions that can be performed in the Accounting Period workspace.
 */
export type AccountingPeriodWorkspaceAction =
  (typeof accountingPeriodWorkspaceActions)[number];

export { accountingPeriodWorkspaceActions };
