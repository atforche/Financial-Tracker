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
import type {
  TransactionAccountingPeriodSummary,
  TransactionDateSummary,
} from "@/transactions/transaction";
import type { JSX } from "react";
import dayjs from "dayjs";

type TransactionTrendsCountChartMode = "AccountingPeriod" | "Date";

interface TransactionTrendsCountChartProps {
  readonly mode: TransactionTrendsCountChartMode;
  readonly accountingPeriods: readonly TransactionAccountingPeriodSummary[] | null;
  readonly dates: readonly TransactionDateSummary[] | null;
}

interface ChartPoint {
  readonly key: string;
  readonly tickLabel: string;
  readonly tooltipLabel: string;
  readonly count: number;
  readonly fill: string;
}

interface ChartPointCandidate {
  readonly key?: unknown;
  readonly tickLabel?: unknown;
  readonly tooltipLabel?: unknown;
  readonly count?: unknown;
  readonly fill?: unknown;
}

const chartColor = "#1976d2";

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
    count: value["count"],
    fill: value["fill"],
  };

  return (
    typeof candidate.key === "string" &&
    typeof candidate.tickLabel === "string" &&
    typeof candidate.tooltipLabel === "string" &&
    typeof candidate.count === "number" &&
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
    count: value.count,
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

const countFormatter = new Intl.NumberFormat("en-US", {
  notation: "compact",
  maximumFractionDigits: 1,
});

const buildChartPoints = function (
  mode: TransactionTrendsCountChartMode,
  accountingPeriods: readonly TransactionAccountingPeriodSummary[],
  dates: readonly TransactionDateSummary[],
): ChartPoint[] {
  if (mode === "AccountingPeriod") {
    return accountingPeriods.map((accountingPeriod) => ({
      key: accountingPeriod.accountingPeriodId,
      tickLabel: accountingPeriod.accountingPeriodName,
      tooltipLabel: accountingPeriod.accountingPeriodName,
      count: accountingPeriod.totalCount,
      fill: chartColor,
    }));
  }

  return dates.map((dateSummary) => ({
    key: dateSummary.date,
    tickLabel: dayjs(dateSummary.date).format("MMM D"),
    tooltipLabel: dayjs(dateSummary.date).format("MMMM D, YYYY"),
    count: dateSummary.totalCount,
    fill: chartColor,
  }));
};

/**
 * Renders transaction counts for the Transactions trends.
 */
const TransactionTrendsCountChart = function ({
  mode,
  accountingPeriods,
  dates,
}: TransactionTrendsCountChartProps): JSX.Element {
  const chartPoints = buildChartPoints(
    mode,
    accountingPeriods ?? [],
    dates ?? [],
  );

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
          <Typography variant="h5">Transaction Count</Typography>
          <Typography variant="body2" color="text.secondary">
            No transaction counts are available for the selected trends range.
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
        <Typography variant="h5">Transaction Count</Typography>
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
                  value: "Transaction Count",
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
                tickFormatter={(value: number) => countFormatter.format(value)}
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
                          {countFormatter.format(point.count)} transactions
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {point.count === 1
                            ? "1 transaction in this period"
                            : "Transactions in this period"}
                        </Typography>
                      </Stack>
                    </Paper>
                  );
                }}
                cursor={{ fill: "rgba(25, 118, 210, 0.08)" }}
              />
              <Bar dataKey="count" />
            </BarChart>
          </ResponsiveContainer>
        </Box>
      </Stack>
    </Paper>
  );
};

export default TransactionTrendsCountChart;
