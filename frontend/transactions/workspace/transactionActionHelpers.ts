import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";

/**
 * Common error fields returned by transaction server actions.
 */
interface TransactionActionErrorState {
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Maps an API failure to flat form state or throws for an unexpected failure.
 */
const mapTransactionActionError = function <StateField extends string>(
  error: unknown,
  fields: Readonly<Record<string, StateField>>,
): TransactionActionErrorState &
  Readonly<Partial<Record<StateField, string | null>>> {
  if (!isApiError(error)) {
    throw new Error("An unexpected error occurred", { cause: error });
  }

  const mappedError = mapApiValidationError(error, fields);
  return {
    errorTitle: mappedError.errorTitle,
    unmappedErrors: mappedError.unmappedErrors,
    ...mappedError.fieldErrors,
  };
};

export { mapTransactionActionError, type TransactionActionErrorState };
