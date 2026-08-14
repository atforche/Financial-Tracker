"use client";

import ComparisonBarPair from "@/framework/view/ComparisonBarPair";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import SummaryCard from "@/framework/view/SummaryCard";

/**
 * Props for the ExpectedGoalContributionsActualCard component.
 */
interface ExpectedGoalContributionsActualCardProps {
  readonly expectedGoalContributions: number | undefined;
  readonly actualGoalContributions: number | undefined;
}

const getFiniteAmount = (amount: number | undefined): number =>
  typeof amount === "number" && Number.isFinite(amount) ? amount : 0;

/**
 * Displays expected Fund Goal contributions against actual contributions.
 */
const ExpectedGoalContributionsActualCard = function ({
  expectedGoalContributions,
  actualGoalContributions,
}: ExpectedGoalContributionsActualCardProps): JSX.Element {
  return (
    <SummaryCard title="Expected Goal Contributions vs. Actual">
      <Stack spacing={2}>
        <ComparisonBarPair
          first={{
            label: "Expected contributions",
            amount: getFiniteAmount(expectedGoalContributions),
            color: "info.main",
            differenceLabel: "Surplus",
            differenceColor: "success.main",
          }}
          second={{
            label: "Actual contributions",
            amount: getFiniteAmount(actualGoalContributions),
            color: "success.main",
            differenceLabel: "Shortfall",
            differenceColor: "error.main",
          }}
        />
      </Stack>
    </SummaryCard>
  );
};

export type { ExpectedGoalContributionsActualCardProps };
export default ExpectedGoalContributionsActualCard;
