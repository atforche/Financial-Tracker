"use client";

import ComparisonBarPair from "@/framework/view/ComparisonBarPair";
import type { IncomeAmount } from "@/transactions/types";
import IncomeBreakdownBar from "@/framework/view/IncomeBreakdownBar";
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
  return (
    <SummaryCard title="Income vs. Spending">
      <Stack spacing={2}>
        <IncomeBreakdownBar
          total={totalIncome.total}
          tracked={totalIncome.tracked}
          untracked={totalIncome.untracked}
        />
        <ComparisonBarPair
          first={{
            label: "Tracked income",
            amount: totalIncome.tracked,
            color: "success.main",
            differenceLabel: "Shortfall",
            differenceColor: "error.main",
          }}
          second={{
            label: "Spending",
            amount: totalSpending,
            color: "error.main",
            differenceLabel: "Remaining",
            differenceColor: "success.main",
          }}
        />
      </Stack>
    </SummaryCard>
  );
};

export type { IncomeSpendingCardProps };
export default IncomeSpendingCard;
