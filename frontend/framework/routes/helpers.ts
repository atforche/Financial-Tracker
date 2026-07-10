/**
 * Checks whether a value is a repeated search param.
 */
const isRepeatedSearchParamArray = function (
  value: string | readonly string[] | undefined,
): value is readonly string[] {
  return Array.isArray(value);
};

/**
 * Normalizes the provided value to a repeated search param.
 */
const toRepeatedSearchParam = function (
  value: string | readonly string[] | undefined,
): string[] {
  return isRepeatedSearchParamArray(value)
    ? [...value]
    : typeof value === "string"
      ? [value]
      : [];
};

/**
 * Builds a URL by combining the provided path name and URL search params.
 */
const buildUrl = function (pathname: string, params: URLSearchParams): string {
  const query = params.toString();
  return query === "" ? pathname : `${pathname}?${query}`;
};

export { toRepeatedSearchParam, buildUrl };
