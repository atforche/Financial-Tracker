/**
 * Retrieves the name of a property from a type in a type-safe manner.
 * @param name - The name of the property to retrieve.
 * @returns The name of the property as a string.
 */
const nameof = function <T>(name: keyof T): string {
  return name.toString();
};

export default nameof;
