import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";

/**
 * Common error fields returned by server actions.
 */
interface ActionErrorState {
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Maps an API failure to flat form state or throws for an unexpected failure.
 */
const mapActionError = function <StateField extends string = never>(
  error: unknown,
  fields: Readonly<Record<string, StateField>>,
): ActionErrorState &
  Readonly<Partial<Record<NoInfer<StateField>, string | null>>> {
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

export type { ActionErrorState };
export default mapActionError;
