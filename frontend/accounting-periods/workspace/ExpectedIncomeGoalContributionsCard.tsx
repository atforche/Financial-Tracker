"use client";

import ComparisonBar from "@/framework/view/ComparisonBar";
import type { IncomeAmount } from "@/transactions/types";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import SummaryCard from "@/framework/view/SummaryCard";

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
  const remaining = trackedIncome - goalContributions;
  const comparisonMax = Math.max(trackedIncome, goalContributions, 1);
  const contributionsAreLesser = goalContributions < trackedIncome;
  const incomeDifference = contributionsAreLesser
    ? 0
    : goalContributions - trackedIncome;
  const contributionsDifference = contributionsAreLesser ? remaining : 0;
  const differenceLabel = remaining >= 0 ? "Remaining" : "Shortfall";
  const differenceColor = remaining >= 0 ? "success.main" : "error.main";

  return (
    <SummaryCard title="Expected Income vs. Goal Contributions">
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
          label="Expected goal contributions"
          amount={goalContributions}
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

export type { ExpectedIncomeGoalContributionsCardProps };
export default ExpectedIncomeGoalContributionsCard;
