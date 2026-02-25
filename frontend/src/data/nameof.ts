/**
 * Retrieves the name of a property from a type in a type-safe manner.
 * @param name - The name of the property to retrieve.
 * @returns The name of the property as a string.
 */
const nameof = <T>(name: keyof T): keyof T => name;

export default nameof;
