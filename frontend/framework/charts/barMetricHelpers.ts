import {
  findTooltipPayload,
  isObject,
} from "@/framework/charts/tooltipHelpers";
import type { ChartColor } from "@/framework/charts/chartTypes";
import type { TooltipContentProps } from "recharts";

/**
 * A normalized data point rendered by the bar metric chart.
 */
interface BarMetricChartPoint {
  readonly tickLabel: string;
  readonly tooltipLabel: string;
  readonly value: number;
  readonly color?: ChartColor;
}

/**
 * Determines if the provided value is a bar metric chart point.
 */
const isBarMetricChartPoint = function (
  value: unknown,
): value is BarMetricChartPoint {
  return (
    isObject(value) &&
    typeof value["tickLabel"] === "string" &&
    typeof value["tooltipLabel"] === "string" &&
    typeof value["value"] === "number" &&
    (typeof value["color"] === "undefined" ||
      (typeof value["color"] === "string" &&
        ["primary", "secondary", "positive", "negative", "neutral"].includes(
          value["color"],
        )))
  );
};

/**
 * Retrieves a normalized bar metric point from a Recharts tooltip payload.
 */
const getBarMetricTooltipChartPoint = function (
  tooltipProps: TooltipContentProps,
): BarMetricChartPoint | null {
  return findTooltipPayload(tooltipProps, isBarMetricChartPoint);
};

/**
 * Gets the semantic bar color for a signed value.
 */
const getSignedChartColor = function (value: number): ChartColor {
  if (value > 0) {
    return "positive";
  }
  if (value < 0) {
    return "negative";
  }
  return "neutral";
};

export { getBarMetricTooltipChartPoint, getSignedChartColor };
export type { BarMetricChartPoint };
