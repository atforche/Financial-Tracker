import type { BalanceEventTotals } from "@/balance-events/types";
import type { EntityHistorySummary } from "@/balance-events/EntityHistoryLayout";

/**
 * Summarizes a date range from its totals, using the current balance when the range has no balances of its own.
 */
const getDateSummary = function (
  totals: BalanceEventTotals,
  currentBalance: number,
): EntityHistorySummary {
  const dates = totals.dates ?? [];
  const closingBalance = dates.at(-1)?.totalBalance ?? currentBalance;
  return {
    openingBalance:
      totals.openingBalance ?? dates.at(0)?.totalBalance ?? closingBalance,
    closingBalance,
    totals,
  };
};

export default getDateSummary;
