/**
 * Error raised when an API request does not return usable data.
 */
class ApiRequestError extends Error {
  /**
   * Creates an API request error with the original response error attached.
   */
  public constructor(message: string, cause: unknown) {
    super(message, { cause });
    this.name = "ApiRequestError";
  }
}

/**
 * Extracts data from a successful API response or throws a request error.
 */
const unwrapApiResponse = function <T>(
  response: { readonly data?: T; readonly error?: unknown },
  errorMessage: string,
): T {
  if (
    typeof response.error !== "undefined" ||
    typeof response.data === "undefined"
  ) {
    throw new ApiRequestError(errorMessage, response.error);
  }

  return response.data;
};

export { ApiRequestError };
export default unwrapApiResponse;
