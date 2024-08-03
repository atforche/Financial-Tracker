/**
 * Ensures that the provided item is not null.
 * @param {T | null} item - Item to check for nullability.
 * @param {string} message - Error message that should be thrown if the provided item is null.
 * @returns {T} The item as a non-nullable type.
 * @throws An error if the provided item is null.
 */
const ensureNotNull = function <T>(item: T | null, message?: string): T {
  if (item === null) {
    throw new Error(message ?? "item is undefined");
  }
  return item;
};

/**
 * If the provided string is blank, returns null. Otherwise, returns the provided string.
 * @param {string} value - String value to check if blank.
 * @returns {string | null} Null if the provided string is blank, otherwise the provided string.
 */
const toNullIfBlank = function (value: string): string | null {
  if (value === "") {
    return null;
  }
  return value;
};

export { ensureNotNull, toNullIfBlank };
