/**
 * Determines whether a value is neither null nor undefined.
 */
const isNotNullOrUndefined = function <T>(
  value: T,
): value is NonNullable<T> {
  return value !== null && typeof value !== "undefined";
};

export default isNotNullOrUndefined;
