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
  type BalanceTrendChartMode,
  type BalanceTrendDateSummary,
  type BalanceTrendPeriodSummary,
  buildBalanceTrendChartPoints,
  compactCurrencyFormatter,
  getTooltipChartPoint,
} from "@/framework/charts/helpers";
import { Box, Paper, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the BalanceTrendChart component.
 */
interface BalanceTrendChartProps {
  readonly mode: BalanceTrendChartMode;
  readonly accountingPeriods: readonly BalanceTrendPeriodSummary[] | null;
  readonly dates: readonly BalanceTrendDateSummary[] | null;
}

/**
 * Renders a balance trend from normalized date or accounting period summaries.
 */
const BalanceTrendChart = function ({
  mode,
  accountingPeriods,
  dates,
}: BalanceTrendChartProps): JSX.Element {
  const chartPoints = buildBalanceTrendChartPoints({
    mode,
    accountingPeriods,
    dates,
  });

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

  const firstPoint = chartPoints.at(0);
  const lastPoint = chartPoints.at(-1);
  if (typeof firstPoint === "undefined" || typeof lastPoint === "undefined") {
    throw new Error("Failed to build account overview trend chart");
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
        <Stack
          direction={{ xs: "column", md: "row" }}
          justifyContent="space-between"
          spacing={1.5}
        >
          <Typography variant="h5">Balance Trend</Typography>
        </Stack>
        <Box sx={{ height: 320, width: "100%" }}>
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart
              data={chartPoints}
              margin={{ top: 12, right: 12, bottom: 24, left: 12 }}
            >
              <defs>
                <linearGradient
                  id="account-overview-trend-fill"
                  x1="0"
                  x2="0"
                  y1="0"
                  y2="1"
                >
                  <stop offset="0%" stopColor="rgba(25, 118, 210, 0.28)" />
                  <stop offset="100%" stopColor="rgba(25, 118, 210, 0.02)" />
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
                  value: mode === "Date" ? "Date" : "Accounting Period",
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
                tickFormatter={(value: number) =>
                  compactCurrencyFormatter.format(value)
                }
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
                cursor={{ stroke: "rgba(25, 118, 210, 0.24)", strokeWidth: 1 }}
              />
              <Area
                dataKey="balance"
                fill="url(#account-overview-trend-fill)"
                stroke="none"
                type="monotone"
              />
              <Line
                activeDot={{
                  fill: "#1976d2",
                  r: 6,
                  stroke: "#ffffff",
                  strokeWidth: 2,
                }}
                dataKey="balance"
                dot={false}
                stroke="#1976d2"
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
