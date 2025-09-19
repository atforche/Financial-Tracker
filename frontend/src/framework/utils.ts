/**
 * Ensures that the provided item is not null.
 * @param {T | null} item - Item to check for nullability.
 * @param {string} message - Error message that should be thrown if the provided item is null.
 * @returns {T} The item as a non-nullable type.
 * @throws An error if the provided item is null.
 */
const ensureNotNull = function <T>(item: T | null, message: string): T {
  if (item === null) {
    throw new Error(message);
  }
  return item;
};

export { ensureNotNull };
