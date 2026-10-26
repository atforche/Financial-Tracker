import enumValues from "@/framework/data/enumValues";

/**
 * Parses a string as a member of a string enum, returning null when invalid.
 */
const parseEnumValue = function <T extends Record<string, string>>(
  enumObject: T,
  value: string,
): T[keyof T] | null {
  return enumValues(enumObject).find((member) => member === value) ?? null;
};

export default parseEnumValue;
