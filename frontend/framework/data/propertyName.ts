/**
 * Returns a string property name while preserving its type.
 */
const propertyName = function <T>(
  name: Extract<keyof T, string>,
): Extract<keyof T, string> {
  return name;
};

export default propertyName;
