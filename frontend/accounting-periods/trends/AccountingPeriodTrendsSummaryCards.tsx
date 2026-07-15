"use client";

import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import BalanceChangeSummaryCards from "@/framework/view/BalanceChangeSummaryCards";
import type { JSX } from "react";

/**
 * Props for the AccountingPeriodTrendsSummaryCards component.
 */
interface AccountingPeriodTrendsSummaryCardsProps {
  readonly accountingPeriods: AccountingPeriodWithBalance[];
}

/**
 * Displays the top-level accounting period balance summary cards.
 */
const AccountingPeriodTrendsSummaryCards = function ({
  accountingPeriods,
}: AccountingPeriodTrendsSummaryCardsProps): JSX.Element {
  const firstPeriod = accountingPeriods.at(0);
  const lastPeriod = accountingPeriods.at(-1);
  const snapshot =
    typeof firstPeriod === "undefined" || typeof lastPeriod === "undefined"
      ? {
          startLabel: "Start",
          endLabel: "End",
          totalStartingBalance: 0,
          totalEndingBalance: 0,
        }
      : {
          startLabel: firstPeriod.name,
          endLabel: lastPeriod.name,
          totalStartingBalance: firstPeriod.openingBalance,
          totalEndingBalance: lastPeriod.closingBalance,
        };

  return (
    <BalanceChangeSummaryCards
      startingTitle={`Starting balance (${snapshot.startLabel})`}
      endingTitle={`Ending balance (${snapshot.endLabel})`}
      startingBalance={snapshot.totalStartingBalance}
      endingBalance={snapshot.totalEndingBalance}
    />
  );
};

export default AccountingPeriodTrendsSummaryCards;
