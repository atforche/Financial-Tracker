/**
 * Determines whether a value is null or undefined.
 */
const isNullOrUndefined = function <T>(
  value: T | null | undefined,
): value is null | undefined {
  return value === null || typeof value === "undefined";
};

/**
 * Determines whether a value is neither null nor undefined.
 */
const isNotNullOrUndefined = function <T>(value: T): value is NonNullable<T> {
  return value !== null && typeof value !== "undefined";
};

export { isNullOrUndefined, isNotNullOrUndefined };
