import type { TooltipContentProps } from "recharts";
import formatCurrency from "@/framework/formatCurrency";

interface BarMetricChartPoint {
  readonly key: string;
  readonly tickLabel: string;
  readonly tooltipLabel: string;
  readonly value: number;
  readonly fill?: string;
}

const positiveBarColor = "#2e7d32";
const negativeBarColor = "#c62828";
const neutralBarColor = "#90a4ae";

const isObject = function (
  value: unknown,
): value is Record<PropertyKey, unknown> {
  return typeof value === "object" && value !== null;
};

const isBarMetricChartPoint = function (
  value: unknown,
): value is BarMetricChartPoint {
  return (
    isObject(value) &&
    typeof value["key"] === "string" &&
    typeof value["tickLabel"] === "string" &&
    typeof value["tooltipLabel"] === "string" &&
    typeof value["value"] === "number" &&
    (typeof value["fill"] === "undefined" ||
      typeof value["fill"] === "string")
  );
};

/** Retrieves a normalized bar metric point from a Recharts tooltip payload. */
const getBarMetricTooltipChartPoint = function (
  tooltipProps: TooltipContentProps,
): BarMetricChartPoint | null {
  for (const payloadEntry of tooltipProps.payload) {
    if (isBarMetricChartPoint(payloadEntry.payload)) {
      return payloadEntry.payload;
    }
  }

  return null;
};

/** Formats a currency value with an explicit sign when it is non-zero. */
const formatSignedCurrency = function (value: number): string {
  if (value === 0) {
    return formatCurrency(value);
  }

  return `${value > 0 ? "+" : "-"}${formatCurrency(Math.abs(value))}`;
};

/** Gets the semantic bar color for a signed value. */
const getSignedBarColor = function (value: number): string {
  if (value > 0) {
    return positiveBarColor;
  }
  if (value < 0) {
    return negativeBarColor;
  }
  return neutralBarColor;
};

export {
  formatSignedCurrency,
  getBarMetricTooltipChartPoint,
  getSignedBarColor,
};
export type { BarMetricChartPoint };
