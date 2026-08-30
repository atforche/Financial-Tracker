"use client";

import ComparisonBarPair from "@/framework/view/ComparisonBarPair";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import SummaryCard from "@/framework/view/SummaryCard";

/**
 * Props for the ExpectedFundGoalContributionsActualCard component.
 */
interface ExpectedFundGoalContributionsActualCardProps {
  readonly expectedFundGoalContributions: number | undefined;
  readonly actualFundGoalContributions: number | undefined;
}

const getFiniteAmount = (amount: number | undefined): number =>
  typeof amount === "number" && Number.isFinite(amount) ? amount : 0;

/**
 * Displays expected Fund Goal contributions against actual contributions.
 */
const ExpectedFundGoalContributionsActualCard = function ({
  expectedFundGoalContributions,
  actualFundGoalContributions,
}: ExpectedFundGoalContributionsActualCardProps): JSX.Element {
  return (
    <SummaryCard title="Expected Fund Goal Contributions vs. Actual">
      <Stack spacing={2}>
        <ComparisonBarPair
          first={{
            label: "Expected contributions",
            amount: getFiniteAmount(expectedFundGoalContributions),
            color: "info.main",
            differenceLabel: "Surplus",
            differenceColor: "success.main",
          }}
          second={{
            label: "Actual contributions",
            amount: getFiniteAmount(actualFundGoalContributions),
            color: "success.main",
            differenceLabel: "Shortfall",
            differenceColor: "error.main",
          }}
        />
      </Stack>
    </SummaryCard>
  );
};

export type { ExpectedFundGoalContributionsActualCardProps };
export default ExpectedFundGoalContributionsActualCard;
