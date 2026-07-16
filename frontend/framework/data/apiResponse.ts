/**
 * Error raised when an API request does not return usable data.
 */
class ApiRequestError extends Error {
  /**
   * Creates an API request error with the original response error attached.
   * @param message - Description of the request that failed.
   * @param cause - Error returned by the API client.
   */
  public constructor(message: string, cause: unknown) {
    super(message, { cause });
    this.name = "ApiRequestError";
  }
}

/**
 * Gets the data from a successful API response.
 * @param response - Response returned by the API client.
 * @param errorMessage - Description of the request to use when it fails.
 * @returns The response data.
 * @throws {ApiRequestError} When the response contains an error or no data.
 */
const getApiData = function <T>(
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

export default getApiData;
