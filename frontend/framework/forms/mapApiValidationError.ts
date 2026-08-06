import type { ApiError } from "@/framework/data/apiError";
import formatErrors from "@/framework/forms/formatErrors";

/**
 * Represents the mapped API validation error for a form, including field errors and unmapped errors.
 */
interface MappedApiValidationError<StateField extends string> {
  readonly errorTitle: string | null;
  readonly fieldErrors: Readonly<Partial<Record<StateField, string | null>>>;
  readonly unmappedErrors: string | null;
}

/**
 * Maps API validation fields to form-state fields and collects other messages.
 */
const mapApiValidationError = function <StateField extends string>(
  error: ApiError,
  fields: Readonly<Record<string, StateField>>,
): MappedApiValidationError<StateField> {
  const normalizedFields = new Map(
    Object.entries(fields).map(([requestField, stateField]) => [
      requestField.toUpperCase(),
      stateField,
    ]),
  );
  const fieldErrors: Partial<Record<StateField, string | null>> = {};
  const unmappedErrors: string[] = [];

  for (const [key, errors] of Object.entries(error.errors ?? {})) {
    const message = formatErrors(errors);
    const stateField = normalizedFields.get(key.toUpperCase());
    if (typeof stateField === "undefined") {
      if (message !== null) {
        unmappedErrors.push(message);
      }
    } else {
      fieldErrors[stateField] = message;
    }
  }

  return {
    errorTitle: error.title ?? "Request failed",
    fieldErrors,
    unmappedErrors:
      formatErrors(unmappedErrors) ??
      "The request could not be completed. Your access may have changed; refresh the page and try again.",
  };
};

export default mapApiValidationError;
