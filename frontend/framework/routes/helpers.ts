type RepeatedSearchParamValue = string | number;
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

export {
  appendRepeatedSearchParam,
  buildUrl,
  compactSearchParams,
  toRepeatedSearchParams,
};
