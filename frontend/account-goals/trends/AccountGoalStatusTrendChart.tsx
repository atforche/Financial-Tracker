/* eslint-disable sort-imports, @typescript-eslint/no-unsafe-assignment, @typescript-eslint/no-unnecessary-condition */
"use client";

import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  type TooltipContentProps,
  XAxis,
  YAxis,
} from "recharts";
import { alpha, useTheme } from "@mui/material/styles";
import {
  chartMargin,
  xAxisTick,
  yAxisTick,
} from "@/framework/charts/chartStyles";
import ChartFrame from "@/framework/charts/ChartFrame";
import ChartTooltip from "@/framework/charts/ChartTooltip";
import { isObject } from "@/framework/charts/tooltipHelpers";
import type { AccountGoalTrendPoint } from "@/account-goals/trends/accountGoalProgressTrends";
import type { JSX } from "react";

/**
 * Renders Account Goal balance-status counts by Accounting Period.
 */
const AccountGoalStatusTrendChart = function ({
  chartPoints,
}: {
  readonly chartPoints: readonly AccountGoalTrendPoint[];
}): JSX.Element {
  const theme = useTheme();
  return (
    <ChartFrame
      title="Account Goal Balance Status"
      emptyMessage="No Account Goal status is available for the selected trends range."
      hasData={chartPoints.some((point) => point.configuredGoalCount > 0)}
      xAxisLabel="Accounting Period"
      yAxisLabel="Account Goals"
    >
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={chartPoints} margin={chartMargin}>
          <CartesianGrid
            strokeDasharray="4 4"
            vertical={false}
            opacity={0.24}
          />
          <XAxis
            axisLine={false}
            dataKey="accountingPeriodName"
            interval="preserveStartEnd"
            minTickGap={24}
            tick={xAxisTick}
            tickLine={false}
          />
          <YAxis
            axisLine={false}
            allowDecimals={false}
            tick={yAxisTick}
            tickLine={false}
            width="auto"
          />
          <Tooltip
            content={(props: TooltipContentProps) => {
              const payload = props.payload?.[0]?.payload;
              const point =
                isObject(payload) &&
                typeof payload["accountingPeriodName"] === "string" &&
                typeof payload["belowZeroCount"] === "number" &&
                typeof payload["belowMinimumCount"] === "number" &&
                typeof payload["withinRangeCount"] === "number" &&
                typeof payload["aboveMaximumCount"] === "number"
                  ? {
                      accountingPeriodName: payload["accountingPeriodName"],
                      belowZeroCount: payload["belowZeroCount"],
                      belowMinimumCount: payload["belowMinimumCount"],
                      withinRangeCount: payload["withinRangeCount"],
                      aboveMaximumCount: payload["aboveMaximumCount"],
                    }
                  : null;
              return !props.active || point === null ? null : (
                <ChartTooltip
                  label={point.accountingPeriodName}
                  value={`Below zero: ${point.belowZeroCount}`}
                  description={`Below minimum: ${point.belowMinimumCount}; Within range: ${point.withinRangeCount}; Above maximum: ${point.aboveMaximumCount}`}
                />
              );
            }}
            cursor={{ fill: alpha(theme.palette.primary.main, 0.08) }}
          />
          <Bar
            dataKey="belowZeroCount"
            stackId="status"
            fill={theme.palette.error.dark}
            name="Below zero"
          />
          <Bar
            dataKey="belowMinimumCount"
            stackId="status"
            fill={theme.palette.warning.main}
            name="Below minimum"
          />
          <Bar
            dataKey="withinRangeCount"
            stackId="status"
            fill={theme.palette.success.main}
            name="Within range"
          />
          <Bar
            dataKey="aboveMaximumCount"
            stackId="status"
            fill={theme.palette.info.main}
            name="Above maximum"
          />
        </BarChart>
      </ResponsiveContainer>
    </ChartFrame>
  );
};

export default AccountGoalStatusTrendChart;
