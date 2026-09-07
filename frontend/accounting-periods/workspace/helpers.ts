/**
 * Possible actions that can be performed in the accounting period workspace.
 */
const accountingPeriodWorkspaceActions = ["create"] as const;

/**
 * Type representing the possible actions that can be performed in the Accounting Period workspace.
 */
type AccountingPeriodWorkspaceAction =
  (typeof accountingPeriodWorkspaceActions)[number];

/**
 * Type representing the possible actions that can be performed in the accounting period workspace.
 */
export type { AccountingPeriodWorkspaceAction };
export { accountingPeriodWorkspaceActions };
