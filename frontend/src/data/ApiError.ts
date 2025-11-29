import type { components } from "@data/api";

/**
 * Type representing an API error response.
 */
type ApiError = components["schemas"]["ErrorModel"];

/**
 * Type representing the details of an API error.
 */
type ApiErrorDetail = components["schemas"]["ErrorDetailModel"];

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
    "message" in obj && typeof obj.message === "string" && "details" in obj
  );
};

export { type ApiError, type ApiErrorDetail, isApiError };
