const longDateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "long",
  day: "numeric",
  year: "numeric",
});

/**
 * Formats a date using the application's long US date presentation.
 * @param date - The date to format.
 * @returns The long date string.
 */
const formatLongDate = function (date: Date): string {
  return longDateFormatter.format(date);
};

export default formatLongDate;
