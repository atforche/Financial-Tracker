"use client";

import ChangeValue from "@/framework/view/ChangeValue";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import SummaryCard from "@/framework/view/SummaryCard";
import SummaryCardGrid from "@/framework/view/SummaryCardGrid";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the BalanceChangeSummaryCards component.
 */
interface BalanceChangeSummaryCardsProps {
  readonly startingTitle: string;
  readonly endingTitle: string;
  readonly startingBalance: number;
  readonly endingBalance: number;
}

/**
 * Displays starting, ending, and net balance change summary cards.
 */
const BalanceChangeSummaryCards = function ({
  startingTitle,
  endingTitle,
  startingBalance,
  endingBalance,
}: BalanceChangeSummaryCardsProps): JSX.Element {
  return (
    <SummaryCardGrid>
      <SummaryCard
        title={startingTitle}
        value={<Stack>{formatCurrency(startingBalance)}</Stack>}
      />
      <SummaryCard
        title={endingTitle}
        value={<Stack>{formatCurrency(endingBalance)}</Stack>}
      />
      <SummaryCard
        title="Net change"
        value={
          <Stack>
            <ChangeValue
              startingValue={startingBalance}
              endingValue={endingBalance}
            />
          </Stack>
        }
      />
    </SummaryCardGrid>
  );
};

export default BalanceChangeSummaryCards;
