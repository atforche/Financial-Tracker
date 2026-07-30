/**
 * Returns the string values of a string enum while preserving its value type.
 */
const enumValues = function <T extends Record<string, string>>(
  enumObject: T,
): readonly T[keyof T][] {
  // eslint-disable-next-line @typescript-eslint/no-unsafe-type-assertion
  return Object.values(enumObject) as T[keyof T][];
};

export default enumValues;
