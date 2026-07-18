import type { ApiError } from "@/framework/data/apiError";
import formatErrors from "@/framework/forms/formatErrors";

/**
 * Represents a formatted error response for account actions, including a title.
 */
interface FormattedAccountActionError {
  readonly errorTitle: string | null;
  readonly fieldErrors: Readonly<Record<string, string | null>>;
  readonly unmappedErrors: string | null;
}

/**
 * Maps an API validation response to account-form fields and collects any
 * remaining validation messages for the form-level alert.
 */
const formatAccountActionError = function (
  error: ApiError,
  fields: Readonly<Record<string, string>>,
): FormattedAccountActionError {
  const normalizedFields = new Map(
    Object.entries(fields).map(([requestField, stateField]) => [
      requestField.toUpperCase(),
      stateField,
    ]),
  );
  const fieldErrors: Record<string, string | null> = {};
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
    errorTitle: error.title ?? null,
    fieldErrors,
    unmappedErrors: unmappedErrors.join(", ") || null,
  };
};

export default formatAccountActionError;
