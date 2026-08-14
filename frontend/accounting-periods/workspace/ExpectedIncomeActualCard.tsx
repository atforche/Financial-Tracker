"use client";

import { Stack, Typography } from "@mui/material";
import ComparisonBarPair from "@/framework/view/ComparisonBarPair";
import type { IncomeAmount } from "@/transactions/types";
import type { JSX } from "react";
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
    label: string,
    expectedAmount: number,
    actualAmount: number,
  ): JSX.Element => (
    <Stack spacing={1}>
      <Typography variant="body2" fontWeight={600}>
        {label}
      </Typography>
      <ComparisonBarPair
        first={{
          label: "Expected income",
          amount: expectedAmount,
          color: "info.main",
          differenceLabel: "Surplus",
          differenceColor: "success.main",
        }}
        second={{
          label: "Actual income",
          amount: actualAmount,
          color: "success.main",
          differenceLabel: "Shortfall",
          differenceColor: "error.main",
        }}
      />
    </Stack>
  );

  return (
    <SummaryCard title="Expected Income vs. Actual">
      <Stack spacing={2}>
        {renderComparisonPair(
          "Tracked income",
          getFiniteAmount(expectedIncome.tracked),
          getFiniteAmount(actualIncome.tracked),
        )}
        {renderComparisonPair(
          "Untracked income",
          getFiniteAmount(expectedIncome.untracked),
          getFiniteAmount(actualIncome.untracked),
        )}
      </Stack>
    </SummaryCard>
  );
};

export type { ExpectedIncomeActualCardProps };
export default ExpectedIncomeActualCard;
