"use client";

import ComparisonBar from "@/transactions/ComparisonBar";
import type { IncomeAmount } from "@/transactions/types";
import IncomeBreakdownBar from "@/transactions/IncomeBreakdownBar";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import SummaryCard from "@/framework/view/SummaryCard";

/**
 * Represents an empty income amount with all values set to zero.
 */
const emptyIncome = {
  total: 0,
  tracked: 0,
  untracked: 0,
};

/**
 * Props for the IncomeSpendingCard component.
 */
interface IncomeSpendingCardProps {
  readonly totalIncome: IncomeAmount | undefined;
  readonly totalSpending: number | undefined;
}

/**
 * Displays total income, its tracked breakdown, and total spending.
 */
const IncomeSpendingCard = function ({
  totalIncome = emptyIncome,
  totalSpending = 0,
}: IncomeSpendingCardProps): JSX.Element {
  const remaining = totalIncome.tracked - totalSpending;
  const comparisonMax = Math.max(totalIncome.tracked, totalSpending, 1);
  const spendingIsLesser = totalSpending < totalIncome.tracked;
  const trackedDifference = spendingIsLesser
    ? 0
    : totalSpending - totalIncome.tracked;
  const spendingDifference = spendingIsLesser ? remaining : 0;
  const differenceLabel = remaining >= 0 ? "Remaining" : "Shortfall";
  const differenceColor = remaining >= 0 ? "success.main" : "error.main";

  return (
    <SummaryCard title="Income vs. spending">
      <Stack spacing={2}>
        <IncomeBreakdownBar
          total={totalIncome.total}
          tracked={totalIncome.tracked}
          untracked={totalIncome.untracked}
        />
        <Stack spacing={1.5}>
          <ComparisonBar
            label="Tracked income"
            amount={totalIncome.tracked}
            amountColor="success.main"
            difference={trackedDifference}
            differenceLabel={differenceLabel}
            differenceColor={differenceColor}
            maxAmount={comparisonMax}
          />
          <ComparisonBar
            label="Spending"
            amount={totalSpending}
            amountColor="error.main"
            difference={spendingDifference}
            differenceLabel={differenceLabel}
            differenceColor={differenceColor}
            maxAmount={comparisonMax}
          />
        </Stack>
      </Stack>
    </SummaryCard>
  );
};

export type { IncomeSpendingCardProps };
export default IncomeSpendingCard;
