"use client";

import ComparisonBarPair from "@/framework/view/ComparisonBarPair";
import type { IncomeAmount } from "@/transactions/types";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import SummaryCard from "@/framework/view/SummaryCard";

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
  const renderComparisonPair = (
    incomeType: "Tracked" | "Untracked",
    expectedAmount: number,
    actualAmount: number,
  ): JSX.Element => (
    <ComparisonBarPair
      first={{
        label: `Expected ${incomeType} Income`,
        amount: expectedAmount,
        color: "info.main",
        differenceLabel: "Surplus",
        differenceColor: "success.main",
      }}
      second={{
        label: `Actual ${incomeType} Income`,
        amount: actualAmount,
        color: "success.main",
        differenceLabel: "Shortfall",
        differenceColor: "error.main",
      }}
    />
  );

  return (
    <SummaryCard title="Expected Income vs. Actual">
      <Stack spacing={2}>
        {renderComparisonPair(
          "Tracked",
          getFiniteAmount(expectedIncome.tracked),
          getFiniteAmount(actualIncome.tracked),
        )}
        {renderComparisonPair(
          "Untracked",
          getFiniteAmount(expectedIncome.untracked),
          getFiniteAmount(actualIncome.untracked),
        )}
      </Stack>
    </SummaryCard>
  );
};

export type { ExpectedIncomeActualCardProps };
export default ExpectedIncomeActualCard;
