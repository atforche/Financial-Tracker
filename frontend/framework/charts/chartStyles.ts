/**
 * Margins to use for charts.
 */
const chartMargin = { top: 12, right: 12, bottom: 24, left: 12 } as const;

/**
 * Font family to use for charts.
 */
const chartFontFamily = '"Roboto", "Helvetica", "Arial", sans-serif';

/**
 * X-axis tick position for charts.
 */
const xAxisTick = { fontFamily: chartFontFamily, dy: 12 } as const;

/**
 * Y-axis tick position for charts.
 */
const yAxisTick = { fontFamily: chartFontFamily, dx: -12 } as const;

export { chartMargin, chartFontFamily, xAxisTick, yAxisTick };
