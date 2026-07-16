import { isNullOrUndefined } from "@/framework/nullHelpers";

/**
 * Converts an arbitrary object to a URLSearchParams instance.
 * Handles nested objects and arrays by serializing them as JSON strings.
 */
const objectToSearchParams = function (
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  obj: Record<string, any>,
): URLSearchParams {
  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(obj)) {
    if (isNullOrUndefined(value)) {
      continue;
    }

    if (typeof value === "object") {
      params.set(key, JSON.stringify(value));
    } else {
      params.set(key, String(value));
    }
  }

  return params;
};

export { objectToSearchParams };
