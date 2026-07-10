const currencyNumberFormatter = new Intl.NumberFormat("en-US", {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

/**
 * Formats the provided USD amount with two decimal places.
 * @param amount - The currency amount to format.
 * @returns The formatted currency string.
 */
const formatCurrency = function (amount: number): string {
  return `$ ${currencyNumberFormatter.format(amount)}`;
};

export default formatCurrency;
