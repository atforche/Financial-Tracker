import type { components } from "@data/api";

/**
 * Type representing an API error response.
 */
type ApiError = components["schemas"]["ValidationProblemDetails"];

/**
 * Type representing the details of an API error.
 */
type ApiErrorDetail = string[];

/**
 * Checks if the given object is an ApiError.
 * @param obj - The object to check.
 * @returns True if the object is an ApiError, false otherwise.
 */
const isApiError = function (obj: unknown): obj is ApiError {
  if (typeof obj !== "object" || obj === null) {
    return false;
  }
  return (
    "title" in obj &&
    typeof obj.title === "string" &&
    "status" in obj &&
    typeof obj.status === "number" &&
    "errors" in obj &&
    typeof obj.errors === "object"
  );
};

export { type ApiError, type ApiErrorDetail, isApiError };
