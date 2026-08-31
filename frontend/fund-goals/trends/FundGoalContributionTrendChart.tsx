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
import {
  findTooltipPayload,
  isObject,
} from "@/framework/charts/tooltipHelpers";
import {
  formatCompactCurrency,
  formatCurrency,
} from "@/framework/currencyHelpers";
import ChartFrame from "@/framework/charts/ChartFrame";
import ChartTooltip from "@/framework/charts/ChartTooltip";
import type { FundGoalTrendPoint } from "@/fund-goals/trends/fundGoalProgressTrends";
import type { JSX } from "react";

/**
 * Props for the FundGoalContributionTrendChart component.
 */
interface FundGoalContributionTrendChartProps {
  readonly chartPoints: readonly FundGoalTrendPoint[];
}

/**
 * Retrieves a Fund Goal contribution point from a chart tooltip payload.
 */
const getTooltipPoint = function (
  tooltipProps: TooltipContentProps,
): FundGoalTrendPoint | null {
  return findTooltipPayload(
    tooltipProps,
    (value): value is FundGoalTrendPoint =>
      isObject(value) &&
      typeof value["accountingPeriodId"] === "string" &&
      typeof value["accountingPeriodName"] === "string" &&
      typeof value["assignedContribution"] === "number" &&
      typeof value["expectedContribution"] === "number",
  );
};

/**
 * Renders expected and assigned Fund Goal contributions by Accounting Period.
 */
const FundGoalContributionTrendChart = function ({
  chartPoints,
}: FundGoalContributionTrendChartProps): JSX.Element {
  const theme = useTheme();
  const hasData = chartPoints.some(
    (point) =>
      point.assignedContribution !== 0 || point.expectedContribution !== 0,
  );

  return (
    <ChartFrame
      title="Expected Fund Goal Contributions vs. Actual"
      emptyMessage="No Fund Goal contributions are configured for the selected trends range."
      hasData={hasData}
      xAxisLabel="Accounting Period"
      yAxisLabel="Fund Goal Contributions"
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
            tick={yAxisTick}
            tickFormatter={(value) =>
              typeof value === "number" ? formatCompactCurrency(value) : ""
            }
            tickLine={false}
            width="auto"
          />
          <Tooltip
            content={(tooltipProps): JSX.Element | null => {
              const point = getTooltipPoint(tooltipProps);
              if (!tooltipProps.active || point === null) {
                return null;
              }

              return (
                <ChartTooltip
                  label={point.accountingPeriodName}
                  value={`Assigned: ${formatCurrency(point.assignedContribution)}`}
                  description={`Expected: ${formatCurrency(point.expectedContribution)}`}
                />
              );
            }}
            cursor={{ fill: alpha(theme.palette.primary.main, 0.08) }}
          />
          <Bar
            dataKey="expectedContribution"
            fill={theme.palette.info.main}
            name="Expected contributions"
            radius={[4, 4, 0, 0]}
          />
          <Bar
            dataKey="assignedContribution"
            fill={theme.palette.success.main}
            name="Assigned contributions"
            radius={[4, 4, 0, 0]}
          />
        </BarChart>
      </ResponsiveContainer>
    </ChartFrame>
  );
};

export default FundGoalContributionTrendChart;
