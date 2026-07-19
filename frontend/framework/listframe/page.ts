/** Number of rows per page in a list frame. */
const rowsPerPage = 10;

/**
 * Normalizes a page value from the URL query parameters, ensuring it is a positive integer.
 */
const normalizePageValue = function (
  value: number | string | null | undefined,
): number {
  const parsedValue = typeof value === "number" ? value : Number(value);
  return Number.isInteger(parsedValue) && parsedValue > 0 ? parsedValue : 1;
};

/**
 * Converts a one-based URL page value to a valid zero-based pagination index.
 */
const getPaginationIndex = function (
  value: number | string | null | undefined,
  totalCount: number,
): number {
  const lastPageIndex = Math.max(Math.ceil(totalCount / rowsPerPage) - 1, 0);
  return Math.min(normalizePageValue(value) - 1, lastPageIndex);
};

/**
 * Gets the page offset.
 */
const getPageOffset = function (
  value: number | string | null | undefined,
): number {
  return (normalizePageValue(value) - 1) * rowsPerPage;
};

export { normalizePageValue, getPageOffset, getPaginationIndex, rowsPerPage };
