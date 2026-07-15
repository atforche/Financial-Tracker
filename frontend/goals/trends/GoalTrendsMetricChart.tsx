"use client";

import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { Box, Paper, Stack, Typography } from "@mui/material";
import type { GoalAccountingPeriodSummary } from "@/goals/trends/goalTrendsTypes";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface GoalTrendsMetricChartProps {
  readonly title: string;
  readonly subtitle: string;
  readonly label: string;
  readonly accountingPeriods: readonly GoalAccountingPeriodSummary[] | null;
  readonly getValue: (accountingPeriod: GoalAccountingPeriodSummary) => number;
  readonly formatter: (value: number) => string;
  readonly tickFormatter?: (value: number) => string;
}

interface ChartPoint {
  readonly key: string;
  readonly label: string;
  readonly value: number;
}

/**
 * A reusable chart component for displaying a single metric across accounting periods on the goal trends.
 */
const GoalTrendsMetricChart = function ({
  title,
  subtitle,
  label,
  accountingPeriods,
  getValue,
  formatter,
  tickFormatter,
}: GoalTrendsMetricChartProps): JSX.Element {
  const chartPoints = (accountingPeriods ?? []).map((summary) => ({
    key: summary.accountingPeriodId,
    label: summary.accountingPeriodName,
    value: getValue(summary),
  }));

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
          <Typography variant="h5">{title}</Typography>
          <Typography variant="body2" color="text.secondary">
            {subtitle}
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
              <XAxis
                axisLine={false}
                dataKey="label"
                interval="preserveStartEnd"
                label={{
                  value: "Accounting Period",
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
                  value: label,
                  angle: -90,
                  position: "center",
                  dx: -42,
                  style: {
                    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
                  },
                }}
                tick={{
                  fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
                  dx: -12,
                }}
                tickFormatter={
                  tickFormatter ??
                  ((value: number): string => formatCurrency(value))
                }
                tickLine={false}
                width={96}
              />
              <Tooltip
                cursor={{ fill: "rgba(25, 118, 210, 0.08)" }}
                content={({ active, payload }): JSX.Element | null => {
                  if (!active || payload.length === 0) {
                    return null;
                  }

                  // eslint-disable-next-line @typescript-eslint/no-unsafe-type-assertion
                  const point = payload[0]?.payload as ChartPoint | undefined;
                  if (typeof point === "undefined") {
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
                          {point.label}
                        </Typography>
                        <Typography variant="body1">
                          {formatter(point.value)}
                        </Typography>
                      </Stack>
                    </Paper>
                  );
                }}
              />
              <Bar dataKey="value" fill="#1976d2" />
            </BarChart>
          </ResponsiveContainer>
        </Box>
      </Stack>
    </Paper>
  );
};

export default GoalTrendsMetricChart;
