"use client";

import {
  Bar,
  BarChart,
  CartesianGrid,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
  type TooltipContentProps,
  XAxis,
  YAxis,
} from "recharts";
import { Box, Paper, Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface AccountingPeriodDashboardChangeChartProps {
  readonly accountingPeriods: readonly AccountingPeriod[] | null;
}

interface ChartPoint {
  readonly key: string;
  readonly tickLabel: string;
  readonly tooltipLabel: string;
  readonly change: number;
  readonly fill: string;
}

interface ChartPointCandidate {
  readonly key?: unknown;
  readonly tickLabel?: unknown;
  readonly tooltipLabel?: unknown;
  readonly change?: unknown;
  readonly fill?: unknown;
}

const positiveBarColor = "#2e7d32";
const negativeBarColor = "#c62828";
const neutralBarColor = "#90a4ae";

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
    change: value["change"],
    fill: value["fill"],
  };

  return (
    typeof candidate.key === "string" &&
    typeof candidate.tickLabel === "string" &&
    typeof candidate.tooltipLabel === "string" &&
    typeof candidate.change === "number" &&
    typeof candidate.fill === "string"
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
    change: value.change,
    fill: value.fill,
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
  signDisplay: "exceptZero",
});

const formatSignedCurrency = function (value: number): string {
  if (value === 0) {
    return formatCurrency(value);
  }

  return `${value > 0 ? "+" : "-"}${formatCurrency(Math.abs(value))}`;
};

const getBarColor = function (value: number): string {
  if (value > 0) {
    return positiveBarColor;
  }
  if (value < 0) {
    return negativeBarColor;
  }
  return neutralBarColor;
};

const buildChartPoints = function (
  accountingPeriods: readonly AccountingPeriod[],
): ChartPoint[] {
  return accountingPeriods.map((accountingPeriod) => {
    const change =
      accountingPeriod.closingBalance - accountingPeriod.openingBalance;

    return {
      key: accountingPeriod.id,
      tickLabel: accountingPeriod.name,
      tooltipLabel: accountingPeriod.name,
      change,
      fill: getBarColor(change),
    };
  });
};

/**
 * Renders balance changes for the Accounting Periods dashboard.
 */
const AccountingPeriodDashboardChangeChart = function ({
  accountingPeriods,
}: AccountingPeriodDashboardChangeChartProps): JSX.Element {
  const chartPoints = buildChartPoints(accountingPeriods ?? []);

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
          <Typography variant="h5">Balance Change</Typography>
          <Typography variant="body2" color="text.secondary">
            No balance changes are available for the selected dashboard range.
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
        <Typography variant="h5">Balance Change</Typography>
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
              <ReferenceLine
                stroke="rgba(15, 23, 42, 0.2)"
                strokeDasharray="4 4"
                y={0}
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
                  value: "Balance Change",
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
                width={96}
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
                          {formatSignedCurrency(point.change)}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {point.change > 0
                            ? "Increase from opening balance"
                            : point.change < 0
                              ? "Decrease from opening balance"
                              : "No change from opening balance"}
                        </Typography>
                      </Stack>
                    </Paper>
                  );
                }}
                cursor={{ fill: "rgba(25, 118, 210, 0.08)" }}
              />
              <Bar dataKey="change" />
            </BarChart>
          </ResponsiveContainer>
        </Box>
      </Stack>
    </Paper>
  );
};

export default AccountingPeriodDashboardChangeChart;
