/**
 * Formats the provided currency amount to a string with two decimal places and a dollar sign.
 * @param amount - The currency amount to format.
 * @returns The formatted currency string.
 */
const formatCurrency = function (amount: number): string {
  return `$ ${amount.toLocaleString([], {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`;
};

export default formatCurrency;
