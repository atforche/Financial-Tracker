import type { ApiError } from "@/framework/data/apiError";
import formatErrors from "@/framework/forms/formatErrors";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";

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

/**
 * Converts an API error into the common accounting period action state.
 */
const getAccountingPeriodActionError = function (
  error: ApiError,
): AccountingPeriodActionState {
  const unmappedErrors = Object.values(error.errors ?? {})
    .map((errors) => formatErrors(errors))
    .filter(isNotNullOrUndefined);

  return {
    errorTitle: error.title ?? null,
    unmappedErrors: formatErrors(unmappedErrors),
  };
};

export type {
  AccountingPeriodActionPayload,
  AccountingPeriodActionState,
  AccountingPeriodServerAction,
};
export { getAccountingPeriodActionError };
