"use client";

import {
  Area,
  AreaChart,
  CartesianGrid,
  Line,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import {
  type BalanceTrendChartPoint,
  getBalanceTrendTooltipPoint,
} from "@/framework/charts/balanceTrendHelpers";
import { type JSX, type ReactNode, useId } from "react";
import { alpha, useTheme } from "@mui/material/styles";
import {
  chartMargin,
  xAxisTick,
  yAxisTick,
} from "@/framework/charts/chartStyles";
import {
  formatCompactCurrency,
  formatCurrency,
} from "@/framework/currencyHelpers";
import ChartFrame from "@/framework/charts/ChartFrame";
import ChartTooltip from "@/framework/charts/ChartTooltip";

/**
 * Props for the BalanceTrendChart component.
 */
interface BalanceTrendChartProps {
  readonly chartPoints: readonly BalanceTrendChartPoint[];
  readonly xAxisLabel: string;
  readonly title?: string;
  readonly headerContent?: ReactNode;
  readonly emptyMessage?: string;
  readonly yAxisLabel?: string;
  readonly tickFormatter?: (value: number) => string;
  readonly valueFormatter?: (value: number) => string;
  readonly color?: "primary" | "secondary";
}

/**
 * Renders a balance trend from normalized chart points.
 */
const BalanceTrendChart = function ({
  chartPoints,
  xAxisLabel,
  title = "Balance Trend",
  headerContent,
  emptyMessage = "No balance history is available for the selected trends range.",
  yAxisLabel = "Total Balance",
  tickFormatter = formatCompactCurrency,
  valueFormatter = formatCurrency,
  color = "primary",
}: BalanceTrendChartProps): JSX.Element {
  const gradientId = `balance-trend-fill-${useId().replaceAll(":", "")}`;
  const theme = useTheme();
  const lineColor = theme.palette[color].main;
  const chartData = chartPoints.map((point, index) => ({
    ...point,
    xAxisValue: index.toString(),
  }));

  return (
    <ChartFrame
      title={title}
      headerContent={headerContent}
      emptyMessage={emptyMessage}
      hasData={chartPoints.length > 0}
      color={color}
      xAxisLabel={xAxisLabel}
      yAxisLabel={yAxisLabel}
    >
      <ResponsiveContainer width="100%" height="100%">
        <AreaChart data={chartData} margin={chartMargin}>
          <defs>
            <linearGradient id={gradientId} x1="0" x2="0" y1="0" y2="1">
              <stop offset="0%" stopColor={alpha(lineColor, 0.28)} />
              <stop offset="100%" stopColor={alpha(lineColor, 0.02)} />
            </linearGradient>
          </defs>
          <CartesianGrid
            strokeDasharray="4 4"
            vertical={false}
            opacity={0.24}
          />
          <XAxis
            axisLine={false}
            dataKey="xAxisValue"
            interval="preserveStartEnd"
            minTickGap={24}
            tick={xAxisTick}
            tickFormatter={(value: string): string =>
              chartData[Number(value)]?.tickLabel ?? value
            }
            tickLine={false}
          />
          <YAxis
            axisLine={false}
            domain={["auto", "auto"]}
            tick={yAxisTick}
            tickFormatter={tickFormatter}
            tickLine={false}
            width="auto"
          />
          <Tooltip
            content={(tooltipProps): JSX.Element | null => {
              const point = getBalanceTrendTooltipPoint(tooltipProps);
              if (!tooltipProps.active || point === null) {
                return null;
              }

              return (
                <ChartTooltip
                  label={point.tooltipLabel}
                  value={valueFormatter(point.balance)}
                  {...(typeof point.description === "string"
                    ? { description: point.description }
                    : {})}
                />
              );
            }}
            cursor={{ stroke: alpha(lineColor, 0.24), strokeWidth: 1 }}
          />
          <Area
            dataKey="balance"
            fill={`url(#${gradientId})`}
            stroke="none"
            type="monotone"
          />
          <Line
            activeDot={{
              fill: lineColor,
              r: 6,
              stroke: theme.palette.background.paper,
              strokeWidth: 2,
            }}
            dataKey="balance"
            dot={false}
            stroke={lineColor}
            strokeWidth={3}
            type="monotone"
          />
        </AreaChart>
      </ResponsiveContainer>
    </ChartFrame>
  );
};

export default BalanceTrendChart;
