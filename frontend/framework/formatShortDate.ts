const shortDateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "numeric",
  year: "numeric",
});

/**
 * Formats a date using the application's short US date presentation.
 * @param date - The date to format.
 * @returns The short date string.
 */
const formatShortDate = function (date: Date): string {
  return shortDateFormatter.format(date);
};

export default formatShortDate;
