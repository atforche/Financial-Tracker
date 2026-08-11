/** Number of items per page when a list frame is displayed as a table. */
const desktopRowsPerPage = 10;

/** Number of items per page when a list frame is displayed as cards. */
const mobileRowsPerPage = 5;

/**
 * Gets the requested list-frame page size, falling back to the desktop size.
 * Only the supported layouts can choose a page size.
 */
const getRowsPerPage = function (
  value: number | string | null | undefined,
): number {
  return Number(value) === mobileRowsPerPage
    ? mobileRowsPerPage
    : desktopRowsPerPage;
};

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
  rowsPerPage = desktopRowsPerPage,
): number {
  const lastPageIndex = Math.max(Math.ceil(totalCount / rowsPerPage) - 1, 0);
  return Math.min(normalizePageValue(value) - 1, lastPageIndex);
};

/**
 * Gets the page offset.
 */
const getPageOffset = function (
  value: number | string | null | undefined,
  rowsPerPage = desktopRowsPerPage,
): number {
  return (normalizePageValue(value) - 1) * rowsPerPage;
};

export {
  desktopRowsPerPage,
  getPageOffset,
  getPaginationIndex,
  getRowsPerPage,
  mobileRowsPerPage,
  normalizePageValue,
};
