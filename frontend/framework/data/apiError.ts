import type { components } from "@/framework/data/api";

/**
 * Type representing an API error response.
 */
type ApiError = components["schemas"]["ValidationProblemDetails"];

/**
 * Checks whether a value is a validation problem returned by the API.
 */
const isApiError = function (obj: unknown): obj is ApiError {
  if (typeof obj !== "object" || obj === null) {
    return false;
  }
  if (!("errors" in obj)) {
    return false;
  }

  const { errors } = obj;
  return (
    typeof errors === "object" &&
    errors !== null &&
    !Array.isArray(errors) &&
    Object.values(errors).every(
      (details) =>
        Array.isArray(details) &&
        details.every((detail) => typeof detail === "string"),
    )
  );
};

export { isApiError, type ApiError };
