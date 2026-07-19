/**
 * State representing the result of an accounting period action, including success or error information.
 */
interface AccountingPeriodActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for server actions that operate on an accounting period.
 */
interface AccountingPeriodActionPayload {
  readonly accountingPeriodId: string;
  readonly redirectUrl: string;
}

/**
 * Type representing a server action that can be performed on an accounting period.
 */
type AccountingPeriodServerAction = (
  state: AccountingPeriodActionState,
  payload: AccountingPeriodActionPayload,
) => Promise<AccountingPeriodActionState>;

export type {
  AccountingPeriodActionPayload,
  AccountingPeriodActionState,
  AccountingPeriodServerAction,
};
