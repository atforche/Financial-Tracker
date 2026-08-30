"use client";

import ComparisonBar from "@/framework/view/ComparisonBar";
import type { IncomeAmount } from "@/transactions/types";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import SummaryCard from "@/framework/view/SummaryCard";

/**
 * Props for the ExpectedIncomeFundGoalContributionsCard component.
 */
interface ExpectedIncomeFundGoalContributionsCardProps {
  readonly expectedIncome: IncomeAmount | undefined;
  readonly expectedFundGoalContributions: number | undefined;
}

/**
 * Displays expected tracked income against expected Fund Goal contributions.
 */
const ExpectedIncomeFundGoalContributionsCard = function ({
  expectedIncome,
  expectedFundGoalContributions,
}: ExpectedIncomeFundGoalContributionsCardProps): JSX.Element {
  const trackedIncome =
    typeof expectedIncome?.tracked === "number" &&
    Number.isFinite(expectedIncome.tracked)
      ? expectedIncome.tracked
      : 0;
  const fundGoalContributions =
    typeof expectedFundGoalContributions === "number" &&
    Number.isFinite(expectedFundGoalContributions)
      ? expectedFundGoalContributions
      : 0;
  const remaining = trackedIncome - fundGoalContributions;
  const comparisonMax = Math.max(trackedIncome, fundGoalContributions, 1);
  const contributionsAreLesser = fundGoalContributions < trackedIncome;
  const incomeDifference = contributionsAreLesser
    ? 0
    : fundGoalContributions - trackedIncome;
  const contributionsDifference = contributionsAreLesser ? remaining : 0;
  const differenceLabel = remaining >= 0 ? "Remaining" : "Shortfall";
  const differenceColor = remaining >= 0 ? "success.main" : "error.main";

  return (
    <SummaryCard title="Expected Income vs. Fund Goal Contributions">
      <Stack spacing={2}>
        <ComparisonBar
          label="Expected tracked income"
          amount={trackedIncome}
          amountColor="success.main"
          difference={incomeDifference}
          differenceLabel={differenceLabel}
          differenceColor={differenceColor}
          maxAmount={comparisonMax}
        />
        <ComparisonBar
          label="Expected Fund Goal contributions"
          amount={fundGoalContributions}
          amountColor="primary.main"
          difference={contributionsDifference}
          differenceLabel={differenceLabel}
          differenceColor={differenceColor}
          maxAmount={comparisonMax}
        />
      </Stack>
    </SummaryCard>
  );
};

export type { ExpectedIncomeFundGoalContributionsCardProps };
export default ExpectedIncomeFundGoalContributionsCard;
