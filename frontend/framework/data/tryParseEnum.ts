/**
 * Attempts to parse the provided string value to a value of the specified enum type.
 */
const tryParseEnum = function <T extends Record<string, string>>(
  enumObject: T,
  value: string,
): T[keyof T] | null {
  // Check if the string value exists in the enum's values
  if (Object.values(enumObject).includes(value)) {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-type-assertion
    return value as T[keyof T];
  }
  return null;
};

export default tryParseEnum;
