import type { ApiError, ApiErrorDetail } from "@data/ApiError";

/**
 * Capitalizes the first letter of the given string.
 * @param str - The string to capitalize.
 * @returns The input string with the first letter capitalized.
 */
const uppercaseFirstLetter = function (str: string): string {
  if (str.length === 0) {
    return str;
  }
  return str.charAt(0).toUpperCase() + str.slice(1);
};

/**
 * Wraps an ApiError and provides access to individual error details by key,
 * tracking which keys have been accessed.
 */
class ApiErrorHandler {
  private readonly error: ApiError;
  private readonly errors: Record<string, ApiErrorDetail>;
  private readonly accessedKeys: Set<string>;

  /**
   * Creates a new ApiErrorHandler from the given ApiError.
   * @param apiError - The ApiError to wrap.
   */
  public constructor(apiError: ApiError) {
    this.error = apiError;
    this.errors = apiError.errors ?? {};
    this.accessedKeys = new Set();
  }

  /**
   * Handles an error for the given key. If the key is null, returns all unhandled errors.
   * @param key - The error key to handle, or null to handle all unhandled errors.
   * @returns The ApiErrorDetail for the key, or all unhandled errors if the key is null.
   */
  public handleError(key: string | null): ApiErrorDetail | null {
    if (key === null) {
      return this.getUnhandledErrors();
    }
    return this.interceptErrorDetail(key);
  }

  /**
   * Returns the general error description from the ApiError, or null if not present.
   * @returns The general error description, or null if not present.
   */
  public getErrorDescription(): string | null {
    return this.error.title ?? null;
  }

  /**
   * Returns the ApiErrorDetail for the given key, or null if it doesn't exist.
   * Marks the key as accessed.
   * @param key - The error key to look up.
   * @returns The ApiErrorDetail for the key, or null if not found.
   */
  private interceptErrorDetail(key: string): ApiErrorDetail | null {
    const formattedKey = uppercaseFirstLetter(key);
    if (formattedKey in this.errors) {
      this.accessedKeys.add(formattedKey);
      return this.errors[formattedKey] ?? null;
    }
    return null;
  }

  /**
   * Returns all ApiErrorDetails that have not been accessed via interceptErrorDetail.
   * @returns A record of unhandled error keys to their ApiErrorDetail values.
   */
  private getUnhandledErrors(): ApiErrorDetail {
    const unhandled: string[] = [];
    for (const [key, detail] of Object.entries(this.errors)) {
      if (!this.accessedKeys.has(key)) {
        unhandled.push(...detail);
      }
    }
    return unhandled;
  }
}

export default ApiErrorHandler;
