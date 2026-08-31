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
  readonly plannedFundGoalContributions: number | undefined;
  readonly expectedFundGoalContributions: number | undefined;
}

/**
 * Displays expected tracked income against planned and expected Fund Goal contributions.
 */
const ExpectedIncomeFundGoalContributionsCard = function ({
  expectedIncome,
  plannedFundGoalContributions,
  expectedFundGoalContributions,
}: ExpectedIncomeFundGoalContributionsCardProps): JSX.Element {
  const trackedIncome =
    typeof expectedIncome?.tracked === "number" &&
    Number.isFinite(expectedIncome.tracked)
      ? expectedIncome.tracked
      : 0;
  const plannedContributions =
    typeof plannedFundGoalContributions === "number" &&
    Number.isFinite(plannedFundGoalContributions)
      ? plannedFundGoalContributions
      : 0;
  const expectedContributions =
    typeof expectedFundGoalContributions === "number" &&
    Number.isFinite(expectedFundGoalContributions)
      ? expectedFundGoalContributions
      : 0;
  const comparisonMax = Math.max(
    trackedIncome,
    plannedContributions,
    expectedContributions,
    1,
  );
  const getDifference = function (contributions: number): {
    difference: number;
    differenceLabel: string;
    differenceColor: string;
  } {
    const remaining = trackedIncome - contributions;
    return {
      difference: Math.abs(remaining),
      differenceLabel: remaining >= 0 ? "Remaining" : "Shortfall",
      differenceColor: remaining >= 0 ? "success.main" : "error.main",
    };
  };
  const plannedDifference = getDifference(plannedContributions);
  const expectedDifference = getDifference(expectedContributions);

  return (
    <SummaryCard title="Expected Income vs. Fund Goal Contributions">
      <Stack spacing={2}>
        <ComparisonBar
          label="Expected tracked income"
          amount={trackedIncome}
          amountColor="success.main"
          maxAmount={comparisonMax}
        />
        <ComparisonBar
          label="Planned Fund Goal contributions"
          amount={plannedContributions}
          amountColor="primary.main"
          difference={plannedDifference.difference}
          differenceLabel={plannedDifference.differenceLabel}
          differenceColor={plannedDifference.differenceColor}
          maxAmount={comparisonMax}
        />
        <ComparisonBar
          label="Expected Fund Goal contributions"
          amount={expectedContributions}
          amountColor="info.main"
          difference={expectedDifference.difference}
          differenceLabel={expectedDifference.differenceLabel}
          differenceColor={expectedDifference.differenceColor}
          maxAmount={comparisonMax}
        />
      </Stack>
    </SummaryCard>
  );
};

export type { ExpectedIncomeFundGoalContributionsCardProps };
export default ExpectedIncomeFundGoalContributionsCard;
