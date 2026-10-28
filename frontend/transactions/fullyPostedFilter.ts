/**
 * Values of the fully posted filter as they appear in the URL.
 */
type FullyPostedFilterValue = "true" | "false";

/**
 * Normalizes a raw fully posted value from the URL, using the first value when the param repeats, and returning null
 * when it does not narrow the transaction set.
 */
const normalizeFullyPosted = function (
  value: string | readonly string[] | null | undefined,
): FullyPostedFilterValue | null {
  const first = typeof value === "string" ? value : value?.[0];
  const trimmed = first?.trim();
  return trimmed === "true" || trimmed === "false" ? trimmed : null;
};

export { normalizeFullyPosted };
export type { FullyPostedFilterValue };
