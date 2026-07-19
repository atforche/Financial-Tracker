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
  parseCurrencyValue,
  sanitizeCurrencyInput,
};
