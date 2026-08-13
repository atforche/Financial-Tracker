"use client";

import type { IncomeAmount } from "@/transactions/types";
import type { JSX } from "react";
import LabeledAmountBar from "@/framework/view/LabeledAmountBar";
import { Stack } from "@mui/material";
import SummaryCard from "@/framework/view/SummaryCard";
import { formatCurrency } from "@/framework/currencyHelpers";

const emptyIncome: IncomeAmount = {
  total: 0,
  tracked: 0,
  untracked: 0,
};

const getFiniteAmount = (amount: number | undefined): number =>
  typeof amount === "number" && Number.isFinite(amount) ? amount : 0;

/**
 * Props for the ExpectedIncomeActualCard component.
 */
interface ExpectedIncomeActualCardProps {
  readonly expectedIncome: IncomeAmount | undefined;
  readonly actualIncome: IncomeAmount | undefined;
}

/**
 * Displays expected income against actual income.
 */
const ExpectedIncomeActualCard = function ({
  expectedIncome = emptyIncome,
  actualIncome = emptyIncome,
}: ExpectedIncomeActualCardProps): JSX.Element {
  const expectedTotal = getFiniteAmount(expectedIncome.total);
  const actualTotal = getFiniteAmount(actualIncome.total);
  const maxAmount = Math.max(expectedTotal, actualTotal, 1);
  const getRatio = (amount: number): number => amount / maxAmount;
  const renderBreakdown = (
    income: IncomeAmount,
    color: string,
  ): JSX.Element => (
    <Stack spacing={1.5} sx={{ pl: 2 }}>
      <LabeledAmountBar
        label="Tracked income"
        value={formatCurrency(getFiniteAmount(income.tracked))}
        ratio={getRatio(getFiniteAmount(income.tracked))}
        color={color}
        barHeight={12}
        compact
      />
      <LabeledAmountBar
        label="Untracked income"
        value={formatCurrency(getFiniteAmount(income.untracked))}
        ratio={getRatio(getFiniteAmount(income.untracked))}
        color={color}
        barHeight={12}
        compact
      />
    </Stack>
  );

  return (
    <SummaryCard title="Expected income vs. actual">
      <Stack spacing={2}>
        <LabeledAmountBar
          label="Expected income"
          value={formatCurrency(expectedTotal)}
          ratio={getRatio(expectedTotal)}
          color="info.main"
        />
        {renderBreakdown(expectedIncome, "info.main")}
        <LabeledAmountBar
          label="Total income"
          value={formatCurrency(actualTotal)}
          ratio={getRatio(actualTotal)}
          color="success.main"
        />
        {renderBreakdown(actualIncome, "success.main")}
      </Stack>
    </SummaryCard>
  );
};

export type { ExpectedIncomeActualCardProps };
export default ExpectedIncomeActualCard;
