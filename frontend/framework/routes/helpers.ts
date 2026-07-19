import { isNullOrUndefined } from "@/framework/nullHelpers";

type RepeatedSearchParamValue = string | number;
type SearchParamComparisonValue = string | number;
type CompactSearchParams<T extends Record<string, unknown>> = {
  [
    K in keyof T as undefined extends T[K]
      ? never
      : T[K] extends readonly unknown[]
        ? never
        : K
  ]: Exclude<T[K], undefined>;
} & {
  [
    K in keyof T as undefined extends T[K]
      ? K
      : T[K] extends readonly unknown[]
        ? K
        : never
  ]?: Exclude<T[K], undefined>;
};

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
 * Normalizes repeated integer search parameters into a bounded, unique list.
 */
const normalizeIntegerSearchParams = function (
  values: readonly RepeatedSearchParamValue[],
  minimumValue: number,
  maximumValue: number,
): number[] {
  const seenValues = new Set<number>();
  const normalizedValues: number[] = [];

  values.forEach((value) => {
    const normalizedValue = String(value).trim();
    if (!/^-?\d+$/u.test(normalizedValue)) {
      return;
    }

    const parsedValue = Number(normalizedValue);
    if (
      !Number.isSafeInteger(parsedValue) ||
      parsedValue < minimumValue ||
      parsedValue > maximumValue ||
      seenValues.has(parsedValue)
    ) {
      return;
    }

    seenValues.add(parsedValue);
    normalizedValues.push(parsedValue);
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
 * Omits undefined values and empty arrays from a search parameter object.
 */
const compactSearchParams = function <T extends Record<string, unknown>>(
  searchParams: T,
): CompactSearchParams<T> {
  const entries = Object.entries(searchParams).filter(
    ([, value]) =>
      typeof value !== "undefined" &&
      (!Array.isArray(value) || value.length > 0),
  );

  // eslint-disable-next-line @typescript-eslint/no-unsafe-type-assertion
  return Object.fromEntries(entries) as CompactSearchParams<T>;
};

/**
 * Builds a URL by combining the provided path name and URL search params.
 */
const buildUrl = function (pathname: string, params: URLSearchParams): string {
  const query = params.toString();
  return query === "" ? pathname : `${pathname}?${query}`;
};

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

export {
  appendRepeatedSearchParam,
  buildUrl,
  compactSearchParams,
  normalizeIntegerSearchParams,
  normalizeStringSearchParams,
  objectToSearchParams,
  selectAvailableSearchParamValues,
  toRepeatedSearchParams,
};
