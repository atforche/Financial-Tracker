import type { TooltipContentProps } from "recharts";

/**
 * Determines whether a value is a non-null object.
 */
const isObject = function (
  value: unknown,
): value is Record<PropertyKey, unknown> {
  return typeof value === "object" && value !== null;
};

/**
 * Finds the first tooltip payload value accepted by a type guard.
 */
const findTooltipPayload = function <T>(
  tooltipProps: TooltipContentProps,
  isValue: (value: unknown) => value is T,
): T | null {
  for (const payloadEntry of tooltipProps.payload) {
    if (isValue(payloadEntry.payload)) {
      return payloadEntry.payload;
    }
  }

  return null;
};

export { findTooltipPayload, isObject };
