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
  getTooltipChartPoint,
} from "@/framework/charts/helpers";
import { Box, Paper, Stack, Typography } from "@mui/material";
import { type JSX, useId } from "react";
import {
  formatCompactCurrency,
  formatCurrency,
} from "@/framework/currencyHelpers";

/**
 * Props for the BalanceTrendChart component.
 */
interface BalanceTrendChartProps {
  readonly chartPoints: readonly BalanceTrendChartPoint[];
  readonly xAxisLabel: string;
  readonly color?: "primary" | "secondary";
}

const chartColors = {
  primary: {
    fillEnd: "rgba(25, 118, 210, 0.02)",
    fillStart: "rgba(25, 118, 210, 0.28)",
    line: "#1976d2",
    cursor: "rgba(25, 118, 210, 0.24)",
  },
  secondary: {
    fillEnd: "rgba(0, 150, 136, 0.02)",
    fillStart: "rgba(0, 150, 136, 0.28)",
    line: "#009688",
    cursor: "rgba(0, 150, 136, 0.24)",
  },
} as const;

/**
 * Renders a balance trend from normalized chart points.
 */
const BalanceTrendChart = function ({
  chartPoints,
  xAxisLabel,
  color = "primary",
}: BalanceTrendChartProps): JSX.Element {
  const gradientId = `balance-trend-fill-${useId().replaceAll(":", "")}`;
  const colors = chartColors[color];

  if (chartPoints.length === 0) {
    return (
      <Paper
        sx={{
          border: "1px solid",
          borderColor: "divider",
          p: 3,
        }}
      >
        <Stack spacing={1}>
          <Typography variant="h5">Balance trend</Typography>
          <Typography variant="body2" color="text.secondary">
            No balance history is available for the selected trends range.
          </Typography>
        </Stack>
      </Paper>
    );
  }

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        p: 3,
      }}
    >
      <Stack spacing={2.5}>
        <Typography variant="h5">Balance Trend</Typography>
        <Box sx={{ height: 320, width: "100%" }}>
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart
              data={chartPoints}
              margin={{ top: 12, right: 12, bottom: 24, left: 12 }}
            >
              <defs>
                <linearGradient id={gradientId} x1="0" x2="0" y1="0" y2="1">
                  <stop offset="0%" stopColor={colors.fillStart} />
                  <stop offset="100%" stopColor={colors.fillEnd} />
                </linearGradient>
              </defs>
              <CartesianGrid
                strokeDasharray="4 4"
                vertical={false}
                opacity={0.24}
              />
              <XAxis
                axisLine={false}
                dataKey="tickLabel"
                interval="preserveStartEnd"
                label={{
                  value: xAxisLabel,
                  position: "insideBottom",
                  dy: 24,
                  style: {
                    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
                  },
                }}
                minTickGap={24}
                tick={{
                  fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
                  dy: 12,
                }}
                tickLine={false}
              />
              <YAxis
                axisLine={false}
                label={{
                  value: "Total Balance",
                  angle: -90,
                  position: "center",
                  dx: -45,
                  style: {
                    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
                  },
                }}
                tick={{
                  fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
                  dx: -12,
                }}
                tickFormatter={(value: number) => formatCompactCurrency(value)}
                tickLine={false}
                width={80}
              />
              <Tooltip
                content={(tooltipProps): JSX.Element | null => {
                  const point = getTooltipChartPoint(tooltipProps);
                  if (!tooltipProps.active || point === null) {
                    return null;
                  }

                  return (
                    <Paper
                      elevation={3}
                      sx={{
                        border: "1px solid",
                        borderColor: "divider",
                        minWidth: 180,
                        p: 1.5,
                      }}
                    >
                      <Stack spacing={0.5}>
                        <Typography variant="overline" color="text.secondary">
                          {point.tooltipLabel}
                        </Typography>
                        <Typography variant="body1">
                          {formatCurrency(point.balance)}
                        </Typography>
                      </Stack>
                    </Paper>
                  );
                }}
                cursor={{ stroke: colors.cursor, strokeWidth: 1 }}
              />
              <Area
                dataKey="balance"
                fill={`url(#${gradientId})`}
                stroke="none"
                type="monotone"
              />
              <Line
                activeDot={{
                  fill: colors.line,
                  r: 6,
                  stroke: "#ffffff",
                  strokeWidth: 2,
                }}
                dataKey="balance"
                dot={false}
                stroke={colors.line}
                strokeWidth={3}
                type="monotone"
              />
            </AreaChart>
          </ResponsiveContainer>
        </Box>
      </Stack>
    </Paper>
  );
};

export default BalanceTrendChart;
