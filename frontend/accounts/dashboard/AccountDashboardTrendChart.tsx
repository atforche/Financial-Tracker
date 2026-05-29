"use client";

import type {
  AccountDashboardDateSummary,
  AccountDashboardPeriodSummary,
} from "@/accounts/types";
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
import type { JSX } from "react";
import dayjs from "dayjs";
import formatCurrency from "@/framework/formatCurrency";

type AccountDashboardTrendChartMode = "AccountingPeriod" | "Date";

/**
 * Props for the AccountDashboardTrendChart component.
 */
interface AccountDashboardTrendChartProps {
  readonly mode: AccountDashboardTrendChartMode;
  readonly accountingPeriods: readonly AccountDashboardPeriodSummary[] | null;
  readonly dates: readonly AccountDashboardDateSummary[] | null;
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

const buildDateChartPoints = function (
  dates: readonly AccountDashboardDateSummary[],
): ChartPoint[] {
  return dates.map((dateSummary) => ({
    key: dateSummary.date,
    tickLabel: dayjs(dateSummary.date).format("MMMM D"),
    tooltipLabel: dayjs(dateSummary.date).format("MMMM D, YYYY"),
    balance: dateSummary.totalBalance,
  }));
};

const buildAccountingPeriodChartPoints = function (
  accountingPeriods: readonly AccountDashboardPeriodSummary[],
): ChartPoint[] {
  const openingPoints = accountingPeriods.map((accountingPeriod) => ({
    key: `${accountingPeriod.accountingPeriodId}-opening`,
    tickLabel: dayjs(
      new Date(accountingPeriod.year, accountingPeriod.month - 1, 1),
    ).format("MMMM YYYY"),
    tooltipLabel: `${accountingPeriod.accountingPeriodName} opening balance`,
    balance: accountingPeriod.totalOpeningBalance,
  }));

  const lastAccountingPeriod = accountingPeriods.at(-1);
  if (typeof lastAccountingPeriod === "undefined") {
    return openingPoints;
  }

  return [
    ...openingPoints,
    {
      key: `${lastAccountingPeriod.accountingPeriodId}-closing`,
      tickLabel: "End",
      tooltipLabel: `${lastAccountingPeriod.accountingPeriodName} closing balance`,
      balance: lastAccountingPeriod.totalClosingBalance,
    },
  ];
};

const buildChartPoints = function ({
  mode,
  accountingPeriods,
  dates,
}: AccountDashboardTrendChartProps): ChartPoint[] {
  if (mode === "Date") {
    return buildDateChartPoints(dates ?? []);
  }

  return buildAccountingPeriodChartPoints(accountingPeriods ?? []);
};

/**
 * Renders the balance trend for the account dashboard.
 */
const AccountDashboardTrendChart = function ({
  mode,
  accountingPeriods,
  dates,
}: AccountDashboardTrendChartProps): JSX.Element {
  const chartPoints = buildChartPoints({
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
            No balance history is available for the selected dashboard range.
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

export default AccountDashboardTrendChart;
