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
import type {
  FundGoalMetricDefinition,
  FundGoalMetricTrendPoint,
} from "@/fund-goals/trends/fundGoalProgressTrends";
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
import type { JSX } from "react";

/**
 * Props for the FundGoalMetricTrendChart component.
 */
interface FundGoalMetricTrendChartProps {
  readonly definition: FundGoalMetricDefinition;
  readonly chartPoints: readonly FundGoalMetricTrendPoint[];
}

/**
 * Retrieves a Fund Goal metric point from a chart tooltip payload.
 */
const getTooltipPoint = function (
  tooltipProps: TooltipContentProps,
): FundGoalMetricTrendPoint | null {
  return findTooltipPayload(
    tooltipProps,
    (value): value is FundGoalMetricTrendPoint =>
      isObject(value) &&
      typeof value["accountingPeriodId"] === "string" &&
      typeof value["accountingPeriodName"] === "string" &&
      typeof value["currentAmount"] === "number" &&
      typeof value["targetAmount"] === "number" &&
      typeof value["configuredGoalCount"] === "number" &&
      typeof value["satisfiedGoalCount"] === "number" &&
      typeof value["satisfiedPercentage"] === "number",
  );
};

/**
 * Renders collective progress for one configured Fund Goal metric.
 */
const FundGoalMetricTrendChart = function ({
  definition,
  chartPoints,
}: FundGoalMetricTrendChartProps): JSX.Element {
  const theme = useTheme();
  const isPercentage = definition.presentation === "percentage";

  return (
    <ChartFrame
      title={definition.title}
      emptyMessage={definition.emptyMessage}
      hasData={chartPoints.length > 0}
      xAxisLabel="Accounting Period"
      yAxisLabel={definition.yAxisLabel}
    >
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={chartPoints} margin={chartMargin}>
          <CartesianGrid
            strokeDasharray="4 4"
            vertical={false}
            opacity={0.24}
          />
          {isPercentage ? null : (
            <ReferenceLine
              stroke={alpha(theme.palette.text.primary, 0.2)}
              strokeDasharray="4 4"
              y={0}
            />
          )}
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
            {...(isPercentage ? { domain: [0, 100] } : {})}
            tick={yAxisTick}
            tickFormatter={(value) =>
              typeof value !== "number"
                ? ""
                : isPercentage
                  ? `${value}%`
                  : formatCompactCurrency(value)
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
                  value={
                    isPercentage
                      ? `${definition.currentLabel}: ${point.satisfiedPercentage.toFixed(0)}%`
                      : `${definition.currentLabel}: ${formatCurrency(point.currentAmount)}`
                  }
                  description={
                    isPercentage
                      ? `${point.satisfiedGoalCount} of ${point.configuredGoalCount} goals satisfied.`
                      : `${definition.targetLabel}: ${formatCurrency(point.targetAmount)}. ${point.satisfiedGoalCount} of ${point.configuredGoalCount} goals satisfied.`
                  }
                />
              );
            }}
            cursor={{ fill: alpha(theme.palette.primary.main, 0.08) }}
          />
          {isPercentage ? null : (
            <Bar
              dataKey="targetAmount"
              fill={theme.palette.secondary.light}
              name={definition.targetLabel}
              radius={[4, 4, 0, 0]}
            />
          )}
          <Bar
            dataKey={isPercentage ? "satisfiedPercentage" : "currentAmount"}
            fill={theme.palette.primary.main}
            name={definition.currentLabel}
            radius={[4, 4, 0, 0]}
          />
        </BarChart>
      </ResponsiveContainer>
    </ChartFrame>
  );
};

export default FundGoalMetricTrendChart;
