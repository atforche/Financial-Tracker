"use client";

import {
  Bar,
  BarChart,
  CartesianGrid,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import {
  type BarMetricChartPoint,
  getBarMetricTooltipChartPoint,
} from "@/framework/charts/barMetricHelpers";
import { alpha, useTheme } from "@mui/material/styles";
import {
  chartMargin,
  xAxisTick,
  yAxisTick,
} from "@/framework/charts/chartStyles";
import ChartFrame from "@/framework/charts/ChartFrame";
import ChartTooltip from "@/framework/charts/ChartTooltip";
import type { JSX } from "react";
import { getChartColor } from "@/framework/charts/chartTheme";

/**
 * Props for the BarMetricChart component.
 */
interface BarMetricChartProps {
  readonly title: string;
  readonly emptyMessage: string;
  readonly chartPoints: readonly BarMetricChartPoint[];
  readonly xAxisLabel: string;
  readonly yAxisLabel: string;
  readonly tickFormatter: (value: number) => string;
  readonly valueFormatter: (value: number) => string;
  readonly getTooltipDescription?: (point: BarMetricChartPoint) => string;
  readonly showZeroLine?: boolean;
}

/**
 * Renders a single normalized metric as a bar chart.
 */
const BarMetricChart = function ({
  title,
  emptyMessage,
  chartPoints,
  xAxisLabel,
  yAxisLabel,
  tickFormatter,
  valueFormatter,
  getTooltipDescription,
  showZeroLine = false,
}: BarMetricChartProps): JSX.Element {
  const theme = useTheme();
  const themedChartPoints = chartPoints.map((point) => ({
    ...point,
    fill: getChartColor(theme, point.color ?? "primary"),
  }));

  return (
    <ChartFrame
      title={title}
      emptyMessage={emptyMessage}
      hasData={chartPoints.length > 0}
      xAxisLabel={xAxisLabel}
      yAxisLabel={yAxisLabel}
    >
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={themedChartPoints} margin={chartMargin}>
          <CartesianGrid
            strokeDasharray="4 4"
            vertical={false}
            opacity={0.24}
          />
          {showZeroLine ? (
            <ReferenceLine
              stroke={alpha(theme.palette.text.primary, 0.2)}
              strokeDasharray="4 4"
              y={0}
            />
          ) : null}
          <XAxis
            axisLine={false}
            dataKey="tickLabel"
            interval="preserveStartEnd"
            minTickGap={24}
            tick={xAxisTick}
            tickLine={false}
          />
          <YAxis
            axisLine={false}
            tick={yAxisTick}
            tickFormatter={tickFormatter}
            tickLine={false}
            width="auto"
          />
          <Tooltip
            content={(tooltipProps): JSX.Element | null => {
              const point = getBarMetricTooltipChartPoint(tooltipProps);
              if (!tooltipProps.active || point === null) {
                return null;
              }

              const description = getTooltipDescription?.(point);
              return (
                <ChartTooltip
                  label={point.tooltipLabel}
                  value={valueFormatter(point.value)}
                  description={description}
                />
              );
            }}
            cursor={{ fill: alpha(theme.palette.primary.main, 0.08) }}
          />
          <Bar dataKey="value" />
        </BarChart>
      </ResponsiveContainer>
    </ChartFrame>
  );
};

export default BarMetricChart;
