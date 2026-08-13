"use client";

import type { IncomeAmount } from "@/transactions/types";
import type { JSX } from "react";
import LabeledAmountBar from "@/framework/view/LabeledAmountBar";
import { Stack } from "@mui/material";
import SummaryCard from "@/framework/view/SummaryCard";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the ExpectedIncomeGoalContributionsCard component.
 */
interface ExpectedIncomeGoalContributionsCardProps {
  readonly expectedIncome: IncomeAmount | undefined;
  readonly expectedGoalContributions: number | undefined;
}

/**
 * Displays expected tracked income against expected Fund Goal contributions.
 */
const ExpectedIncomeGoalContributionsCard = function ({
  expectedIncome,
  expectedGoalContributions,
}: ExpectedIncomeGoalContributionsCardProps): JSX.Element {
  const trackedIncome =
    typeof expectedIncome?.tracked === "number" &&
    Number.isFinite(expectedIncome.tracked)
      ? expectedIncome.tracked
      : 0;
  const goalContributions =
    typeof expectedGoalContributions === "number" &&
    Number.isFinite(expectedGoalContributions)
      ? expectedGoalContributions
      : 0;
  const maxAmount = Math.max(trackedIncome, goalContributions, 1);

  return (
    <SummaryCard title="Expected income vs. goal contributions">
      <Stack spacing={2}>
        <LabeledAmountBar
          label="Expected tracked income"
          value={formatCurrency(trackedIncome)}
          ratio={trackedIncome / maxAmount}
          color="info.main"
        />
        <LabeledAmountBar
          label="Expected goal contributions"
          value={formatCurrency(goalContributions)}
          ratio={goalContributions / maxAmount}
          color="warning.main"
        />
      </Stack>
    </SummaryCard>
  );
};

export type { ExpectedIncomeGoalContributionsCardProps };
export default ExpectedIncomeGoalContributionsCard;
