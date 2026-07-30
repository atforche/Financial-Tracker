/**
 * Formatter for long US date presentation (e.g., "January 1, 2023").
 */
const longDateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "long",
  day: "numeric",
  year: "numeric",
});

/**
 * Formatter for short US date presentation (e.g., "Jan 1, 2023").
 */
const shortDateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "numeric",
  year: "numeric",
});

/**
 * Formats a date using the application's long US date presentation.
 */
const formatLongDate = function (date: Date): string {
  return longDateFormatter.format(date);
};

/**
 * Formats a date using the application's short US date presentation.
 */
const formatShortDate = function (date: Date): string {
  return shortDateFormatter.format(date);
};

export { formatLongDate, formatShortDate };
