/**
 * Margins to use for charts.
 */
const chartMargin = { top: 12, right: 12, bottom: 24, left: 12 } as const;

/**
 * Font family to use for charts.
 */
const chartFontFamily = '"Roboto", "Helvetica", "Arial", sans-serif';

/**
 * X-axis label position for charts.
 */
const xAxisLabelPosition = {
  position: "insideBottom" as const,
  dy: 24,
  style: { fontFamily: chartFontFamily },
};

/**
 * X-axis tick position for charts.
 */
const xAxisTick = { fontFamily: chartFontFamily, dy: 12 } as const;

/**
 * Y-axis label position for charts.
 */
const yAxisLabelPosition = {
  angle: -90,
  position: "center" as const,
  dx: -45,
  style: { fontFamily: chartFontFamily },
};

/**
 * Y-axis tick position for charts.
 */
const yAxisTick = { fontFamily: chartFontFamily, dx: -12 } as const;

export {
  chartMargin,
  xAxisLabelPosition,
  xAxisTick,
  yAxisLabelPosition,
  yAxisTick,
};
