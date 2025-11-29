/**
 * Ensures that the provided item is not null.
 * @param item - Item to check for nullability.
 * @param message - Error message that should be thrown if the provided item is null.
 * @returns The item as a non-nullable type.
 * @throws {Error} An error if the provided item is null.
 */
const ensureNotNull = function <T>(item: T | null, message: string): T {
  if (item === null) {
    throw new Error(message);
  }
  return item;
};

export { ensureNotNull };
