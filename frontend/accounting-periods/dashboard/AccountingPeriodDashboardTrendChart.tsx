"use client";

import {
  Area,
  AreaChart,
  CartesianGrid,
  Line,
  ResponsiveContainer,
  Tooltip,
  type TooltipContentProps,
  XAxis,
  YAxis,
} from "recharts";
import { Box, Paper, Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";
import dayjs from "dayjs";
import formatCurrency from "@/framework/formatCurrency";

interface AccountingPeriodDashboardTrendChartProps {
  readonly accountingPeriods: readonly AccountingPeriod[] | null;
}

interface ChartPoint {
  readonly key: string;
  readonly tickLabel: string;
  readonly tooltipLabel: string;
  readonly balance: number;
}

interface ChartPointCandidate {
  readonly key?: unknown;
  readonly tickLabel?: unknown;
  readonly tooltipLabel?: unknown;
  readonly balance?: unknown;
}

const isObject = function (
  value: unknown,
): value is Record<PropertyKey, unknown> {
  return typeof value === "object" && value !== null;
};

const isChartPoint = function (value: unknown): value is ChartPoint {
  if (!isObject(value)) {
    return false;
  }

  const candidate: ChartPointCandidate = {
    key: value["key"],
    tickLabel: value["tickLabel"],
    tooltipLabel: value["tooltipLabel"],
    balance: value["balance"],
  };

  return (
    typeof candidate.key === "string" &&
    typeof candidate.tickLabel === "string" &&
    typeof candidate.tooltipLabel === "string" &&
    typeof candidate.balance === "number"
  );
};

const toChartPoint = function (value: unknown): ChartPoint | null {
  if (!isChartPoint(value)) {
    return null;
  }

  return {
    key: value.key,
    tickLabel: value.tickLabel,
    tooltipLabel: value.tooltipLabel,
    balance: value.balance,
  };
};

const getTooltipChartPoint = function (
  tooltipProps: TooltipContentProps,
): ChartPoint | null {
  for (const payloadEntry of tooltipProps.payload) {
    const chartPoint = toChartPoint(payloadEntry.payload);
    if (chartPoint !== null) {
      return chartPoint;
    }
  }

  return null;
};

const compactCurrencyFormatter = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  notation: "compact",
  maximumFractionDigits: 1,
});

const buildAccountingPeriodChartPoints = function (
  accountingPeriods: readonly AccountingPeriod[],
): ChartPoint[] {
  const openingPoints = accountingPeriods.map((accountingPeriod) => ({
    key: `${accountingPeriod.id}-opening`,
    tickLabel: dayjs(accountingPeriod.name, "MMMM YYYY").format("MMMM YYYY"),
    tooltipLabel: `${accountingPeriod.name} opening balance`,
    balance: accountingPeriod.openingBalance,
  }));

  const lastAccountingPeriod = accountingPeriods.at(-1);
  if (typeof lastAccountingPeriod === "undefined") {
    return openingPoints;
  }

  return [
    ...openingPoints,
    {
      key: `${lastAccountingPeriod.id}-closing`,
      tickLabel: "End",
      tooltipLabel: `${lastAccountingPeriod.name} closing balance`,
      balance: lastAccountingPeriod.closingBalance,
    },
  ];
};

const buildChartPoints = function ({
  accountingPeriods,
}: AccountingPeriodDashboardTrendChartProps): ChartPoint[] {
  return buildAccountingPeriodChartPoints(accountingPeriods ?? []);
};

/**
 * Renders the balance trend for the accounting periods dashboard.
 */
const AccountingPeriodDashboardTrendChart = function ({
  accountingPeriods,
}: AccountingPeriodDashboardTrendChartProps): JSX.Element {
  const chartPoints = buildChartPoints({
    accountingPeriods,
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
            No balance history is available for the selected dashboard range.
          </Typography>
        </Stack>
      </Paper>
    );
  }

  const firstPoint = chartPoints.at(0);
  const lastPoint = chartPoints.at(-1);
  if (typeof firstPoint === "undefined" || typeof lastPoint === "undefined") {
    throw new Error("Failed to build accounting period trend chart");
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
                <linearGradient
                  id="accounting-period-dashboard-trend-fill"
                  x1="0"
                  x2="0"
                  y1="0"
                  y2="1"
                >
                  <stop offset="0%" stopColor="rgba(0, 150, 136, 0.28)" />
                  <stop offset="100%" stopColor="rgba(0, 150, 136, 0.02)" />
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
                content={(
                  tooltipProps: TooltipContentProps,
                ): JSX.Element | null => {
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
                cursor={{ stroke: "rgba(0, 150, 136, 0.24)", strokeWidth: 1 }}
              />
              <Area
                dataKey="balance"
                fill="url(#accounting-period-dashboard-trend-fill)"
                fillOpacity={1}
                stroke="none"
                type="monotone"
              />
              <Line
                activeDot={{
                  fill: "#009688",
                  r: 6,
                  stroke: "#ffffff",
                  strokeWidth: 2,
                }}
                dataKey="balance"
                dot={false}
                stroke="#009688"
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

export default AccountingPeriodDashboardTrendChart;
