"use client";

import type { IncomeAmount } from "@/framework/data/types";
import type { JSX } from "react";
import LabeledAmountBar from "@/framework/view/LabeledAmountBar";
import { Stack } from "@mui/material";
import SummaryCard from "@/framework/view/SummaryCard";
import { formatCurrency } from "@/framework/currencyHelpers";

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
  const maxAmount = Math.max(totalIncome.total, totalSpending, 1);
  const trackedIncomeRatio =
    totalIncome.total === 0 ? 0 : totalIncome.tracked / totalIncome.total;
  const untrackedIncomeRatio =
    totalIncome.total === 0 ? 0 : totalIncome.untracked / totalIncome.total;

  return (
    <SummaryCard title="Income vs. spending">
      <Stack spacing={2}>
        <LabeledAmountBar
          label="Total income"
          value={formatCurrency(totalIncome.total)}
          ratio={totalIncome.total / maxAmount}
          color="success.main"
        />
        <Stack spacing={1.5} sx={{ pl: 2 }}>
          <LabeledAmountBar
            label="Tracked income"
            value={formatCurrency(totalIncome.tracked)}
            ratio={trackedIncomeRatio}
            color="success.main"
            barHeight={12}
            compact
          />
          <LabeledAmountBar
            label="Untracked income"
            value={formatCurrency(totalIncome.untracked)}
            ratio={untrackedIncomeRatio}
            color="success.main"
            barHeight={12}
            compact
          />
        </Stack>
        <LabeledAmountBar
          label="Total spending"
          value={formatCurrency(totalSpending)}
          ratio={totalSpending / maxAmount}
          color="error.main"
        />
      </Stack>
    </SummaryCard>
  );
};

export type { IncomeSpendingCardProps };
export default IncomeSpendingCard;
