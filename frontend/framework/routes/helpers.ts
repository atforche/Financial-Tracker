import { isNullOrUndefined } from "@/framework/nullHelpers";

/**
 * A value that can be repeated within a search parameter.
 */
type RepeatedSearchParamValue = string | number;

/**
 * A value that can be compared against a search parameter.
 */
type SearchParamComparisonValue = string | number;

/**
 * Determines if the provided value is a repeated search param array.
 */
const isRepeatedSearchParamArray = function <
  T extends RepeatedSearchParamValue,
>(value: T | readonly T[] | undefined): value is readonly T[] {
  return Array.isArray(value);
};

/**
 * Normalizes a scalar or repeated search parameter to an array.
 */
const toRepeatedSearchParams = function <T extends RepeatedSearchParamValue>(
  value: T | readonly T[] | undefined,
): T[] {
  if (isRepeatedSearchParamArray(value)) {
    return [...value];
  }

  return typeof value === "undefined" ? [] : [value];
};

/**
 * Normalizes repeated string search parameters into a trimmed, unique list.
 */
const normalizeStringSearchParams = function (
  values: readonly string[],
  getComparisonValue: (value: string) => SearchParamComparisonValue = (value) =>
    value,
): string[] {
  const seenValues = new Set<SearchParamComparisonValue>();
  const normalizedValues: string[] = [];

  values.forEach((value) => {
    const normalizedValue = value.trim();
    const comparisonValue = getComparisonValue(normalizedValue);
    if (normalizedValue === "" || seenValues.has(comparisonValue)) {
      return;
    }

    seenValues.add(comparisonValue);
    normalizedValues.push(normalizedValue);
  });

  return normalizedValues;
};

/**
 * Selects available values whose keys occur in the requested values.
 */
const selectAvailableSearchParamValues = function <
  TRequested,
  TAvailable,
  TKey extends SearchParamComparisonValue,
>(
  requestedValues: readonly TRequested[],
  availableValues: readonly TAvailable[],
  getRequestedKey: (value: TRequested) => TKey,
  getAvailableKey: (value: TAvailable) => TKey,
): TAvailable[] {
  const selectedKeys = new Set(requestedValues.map(getRequestedKey));
  if (selectedKeys.size === 0 || availableValues.length === 0) {
    return [];
  }

  return availableValues.filter((value) =>
    selectedKeys.has(getAvailableKey(value)),
  );
};

/**
 * Appends a scalar or repeated value to URL search parameters.
 */
const appendRepeatedSearchParam = function <T extends RepeatedSearchParamValue>(
  params: URLSearchParams,
  key: string,
  value: T | readonly T[] | undefined,
): void {
  toRepeatedSearchParams(value).forEach((item) => {
    params.append(key, String(item));
  });
};

/**
 * Builds a URL by combining the provided path name and URL search params.
 */
const buildUrl = function (pathname: string, params: URLSearchParams): string {
  const query = params.toString();
  return query === "" ? pathname : `${pathname}?${query}`;
};

/**
 * Serializes one non-null search parameter value.
 */
const serializeSearchParamValue = function (value: unknown): string {
  if (typeof value === "object") {
    return JSON.stringify(value);
  }

  if (typeof value === "symbol") {
    return value.description ?? "";
  }

  if (typeof value === "function") {
    return value.name;
  }

  if (typeof value === "undefined") {
    return "";
  }

  if (
    typeof value === "string" ||
    typeof value === "number" ||
    typeof value === "bigint" ||
    typeof value === "boolean"
  ) {
    return String(value);
  }

  return "";
};

/**
 * Converts an arbitrary object to a URLSearchParams instance.
 * Handles nested objects and arrays by serializing them as JSON strings.
 */
const objectToSearchParams = function <T extends object>(
  obj: T,
): URLSearchParams {
  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(obj)) {
    if (isNullOrUndefined(value)) {
      continue;
    }

    if (Array.isArray(value)) {
      value.forEach((item) => {
        if (!isNullOrUndefined(item)) {
          params.append(key, serializeSearchParamValue(item));
        }
      });
    } else if (typeof value === "object") {
      params.set(key, serializeSearchParamValue(value));
    } else {
      params.set(key, serializeSearchParamValue(value));
    }
  }

  return params;
};

export {
  appendRepeatedSearchParam,
  buildUrl,
  normalizeStringSearchParams,
  objectToSearchParams,
  selectAvailableSearchParamValues,
  toRepeatedSearchParams,
};
