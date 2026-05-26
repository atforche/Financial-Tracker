/**
 * Supported trend window sizes for the Accounting Periods dashboard.
 */
const accountingPeriodTrendRanges = [3, 6, 12, 24] as const;

/**
 * Default trend window size for the Accounting Periods dashboard.
 */
const defaultAccountingPeriodTrendRange = 6;

/**
 * Supported trend window size type.
 */
type AccountingPeriodTrendRange = (typeof accountingPeriodTrendRanges)[number];

/**
 * Type guard for supported dashboard trend window sizes.
 */
const isAccountingPeriodTrendRange = function (
  value: number,
): value is AccountingPeriodTrendRange {
  return accountingPeriodTrendRanges.some((trendRange) => trendRange === value);
};

/**
 * Parses the provided trend range into a supported dashboard value.
 */
const parseAccountingPeriodTrendRange = function (
  value: unknown,
): AccountingPeriodTrendRange {
  const parsedValue =
    typeof value === "number"
      ? value
      : typeof value === "string"
        ? Number(value)
        : Number.NaN;

  if (isAccountingPeriodTrendRange(parsedValue)) {
    return parsedValue;
  }

  return defaultAccountingPeriodTrendRange;
};

export {
  accountingPeriodTrendRanges,
  defaultAccountingPeriodTrendRange,
  parseAccountingPeriodTrendRange,
  type AccountingPeriodTrendRange,
};
