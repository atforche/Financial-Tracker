/**
 * Formatter for currency values with two decimal places.
 */
const currencyNumberFormatter = new Intl.NumberFormat("en-US", {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

/**
 * Formatter for currency values with two decimal places, without a currency symbol.
 */
const currencyValueFormatter = new Intl.NumberFormat([], {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

/**
 * Formatter for compact currency values with one decimal place, without a currency symbol.
 */
const compactCurrencyFormatter = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  notation: "compact",
  maximumFractionDigits: 1,
});

/**
 * Formatter for compact currency values with one decimal place, without a currency symbol, and with a sign for non-zero values.
 */
const signedCompactCurrencyFormatter = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  notation: "compact",
  maximumFractionDigits: 1,
  signDisplay: "exceptZero",
});

/**
 * Regular expression pattern for validating currency input in an entry field.
 */
const currencyEditPattern = /^-?\d*(?:\.\d{0,2})?$/u;

/**
 * Formats the provided USD amount with two decimal places.
 */
const formatCurrency = function (amount: number): string {
  return `$ ${currencyNumberFormatter.format(amount)}`;
};

/**
 * Formats a currency value for display in an entry field.
 */
const formatCurrencyValue = function (value: number): string {
  return currencyValueFormatter.format(value);
};

/**
 * Formats a USD amount using compact notation for chart axes.
 */
const formatCompactCurrency = function (
  amount: number,
  showSign = false,
): string {
  return (
    showSign ? signedCompactCurrencyFormatter : compactCurrencyFormatter
  ).format(amount);
};

/**
 * Formats a currency value with an explicit sign when it is non-zero.
 */
const formatSignedCurrency = function (value: number): string {
  if (value === 0) {
    return formatCurrency(value);
  }
  return `${value > 0 ? "+" : "-"}${formatCurrency(Math.abs(value))}`;
};

/**
 * Converts a currency amount to whole cents.
 */
const getCurrencyCents = function (amount: number): number {
  return Math.round(amount * 100);
};

/**
 * Sums currency amounts using whole cents.
 */
const getCurrencyTotal = function (
  amounts: readonly (number | null | undefined)[],
): number {
  const totalInCents = amounts.reduce<number>(
    (total, amount) => total + getCurrencyCents(amount ?? 0),
    0,
  );
  return totalInCents === 0 ? 0 : totalInCents / 100;
};

/**
 * Gets the difference between two currency amounts using whole cents.
 */
const getCurrencyDifference = function (
  sourceAmount: number,
  destinationAmount: number,
): number {
  return getCurrencyTotal([sourceAmount, -destinationAmount]);
};

/**
 * Gets the lower of two currency amounts using whole cents.
 */
const getMinimumCurrencyAmount = function (
  leftAmount: number,
  rightAmount: number,
): number {
  return (
    Math.min(getCurrencyCents(leftAmount), getCurrencyCents(rightAmount)) / 100
  );
};

/**
 * Removes currency display characters from an entry field value.
 */
const sanitizeCurrencyInput = function (value: string): string {
  return value.replace(/[$,\s]/gu, "");
};

/**
 * Parses a currency entry field value, returning null for incomplete input.
 */
const parseCurrencyValue = function (value: string): number | null {
  if (value === "" || value === "-" || value === "." || value === "-.") {
    return null;
  }

  if (!/^-?(?:\d+|\d*\.\d{1,2})$/u.test(value)) {
    return null;
  }

  const parsedValue = Number(value);
  return Number.isNaN(parsedValue) ? null : parsedValue;
};

export {
  currencyEditPattern,
  formatCompactCurrency,
  formatCurrency,
  formatCurrencyValue,
  formatSignedCurrency,
  getCurrencyDifference,
  getCurrencyTotal,
  getMinimumCurrencyAmount,
  parseCurrencyValue,
  sanitizeCurrencyInput,
};
