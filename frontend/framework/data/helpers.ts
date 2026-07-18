import { BalanceEventType } from "@/framework/data/types";

/**
 * Formats a balance event type for display.
 */
const formatBalanceEventType = function (
  type: BalanceEventType,
  isPosted = true,
): string {
  const label = type === BalanceEventType.Debit ? "Debit" : "Credit";
  return isPosted ? label : `Pending ${label.toLowerCase()}`;
};

export { formatBalanceEventType };
