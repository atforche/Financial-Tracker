/**
 * Parses a string as a member of a string enum, returning null when invalid.
 */
const parseEnumValue = function <T extends Record<string, string>>(
  enumObject: T,
  value: string,
): T[keyof T] | null {
  if (Object.values(enumObject).includes(value)) {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-type-assertion
    return value as T[keyof T];
  }
  return null;
};

export default parseEnumValue;
