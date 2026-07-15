import type { AccountingPeriodWithTransactions } from "@/accounting-periods/types";
import BalanceChangeSummaryCards from "@/framework/view/BalanceChangeSummaryCards";
import type { JSX } from "react";

/**
 * Props for the CurrentAccountingPeriodSummaryCards component.
 */
interface AccountingPeriodCurrentSummaryCardsProps {
  readonly current: AccountingPeriodWithTransactions | null;
}

/**
 * Displays the top-level current accounting period balance summary cards.
 */
const CurrentAccountingPeriodSummaryCards = function ({
  current,
}: AccountingPeriodCurrentSummaryCardsProps): JSX.Element {
  const openingBalance = current?.openingBalance ?? 0;
  const closingBalance = current?.closingBalance ?? 0;
  const titleSuffix = current === null ? "" : ` (${current.name})`;

  return (
    <BalanceChangeSummaryCards
      startingTitle={`Opening balance${titleSuffix}`}
      endingTitle={`Closing balance${titleSuffix}`}
      startingBalance={openingBalance}
      endingBalance={closingBalance}
    />
  );
};

export default CurrentAccountingPeriodSummaryCards;
