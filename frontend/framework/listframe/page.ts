import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Normalizes a page value from the URL query parameters, ensuring it is a positive integer.
 */
const normalizePageValue = function (
  value: number | string | null | undefined,
): number {
  if (typeof value === "number" && Number.isFinite(value) && value > 0) {
    return value;
  }
  const parsedValue = Number.parseInt(String(value ?? ""), 10);
  return Number.isFinite(parsedValue) && parsedValue > 0 ? parsedValue : 1;
};

/**
 * Gets the page offset.
 */
const getPageOffset = function (
  value: number | string | null | undefined,
): number {
  return (normalizePageValue(value) - 1) * rowsPerPage;
};

export { normalizePageValue, getPageOffset };
