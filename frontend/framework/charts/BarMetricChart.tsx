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
import { Box, Paper, Stack, Typography } from "@mui/material";
import type { JSX } from "react";

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
  if (chartPoints.length === 0) {
    return (
      <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
        <Stack spacing={1}>
          <Typography variant="h5">{title}</Typography>
          <Typography variant="body2" color="text.secondary">
            {emptyMessage}
          </Typography>
        </Stack>
      </Paper>
    );
  }

  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={2.5}>
        <Typography variant="h5">{title}</Typography>
        <Box sx={{ height: 320, width: "100%" }}>
          <ResponsiveContainer width="100%" height="100%">
            <BarChart
              data={chartPoints}
              margin={{ top: 12, right: 12, bottom: 24, left: 12 }}
            >
              <CartesianGrid
                strokeDasharray="4 4"
                vertical={false}
                opacity={0.24}
              />
              {showZeroLine ? (
                <ReferenceLine
                  stroke="rgba(15, 23, 42, 0.2)"
                  strokeDasharray="4 4"
                  y={0}
                />
              ) : null}
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
                  value: yAxisLabel,
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
                tickFormatter={tickFormatter}
                tickLine={false}
                width={96}
              />
              <Tooltip
                content={(tooltipProps): JSX.Element | null => {
                  const point = getBarMetricTooltipChartPoint(tooltipProps);
                  if (!tooltipProps.active || point === null) {
                    return null;
                  }

                  const description = getTooltipDescription?.(point);
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
                          {valueFormatter(point.value)}
                        </Typography>
                        {typeof description === "undefined" ? null : (
                          <Typography variant="body2" color="text.secondary">
                            {description}
                          </Typography>
                        )}
                      </Stack>
                    </Paper>
                  );
                }}
                cursor={{ fill: "rgba(25, 118, 210, 0.08)" }}
              />
              <Bar dataKey="value" />
            </BarChart>
          </ResponsiveContainer>
        </Box>
      </Stack>
    </Paper>
  );
};

export default BarMetricChart;
