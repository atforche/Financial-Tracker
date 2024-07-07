/**
 * @file Defines shared quality-of-life functions that may be used in many different places throughout the application.
 */

/**
 * Ensures that the provided item is not null.
 * @param {T | null} item - Item to check for nullness.
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

export { ensureNotNull };
