"use client";

import {
  type FundGoalPeriodProgress,
  getFundGoalHealthSummary,
} from "@/fund-goals/trends/fundGoalProgressTrends";
import ComparisonBarPair from "@/framework/view/ComparisonBarPair";
import type { JSX } from "react";
import LabeledAmountBar from "@/framework/view/LabeledAmountBar";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import { Stack } from "@mui/material";
import SummaryCard from "@/framework/view/SummaryCard";

/**
 * Props for the FundGoalTrendsSummaryCards component.
 */
interface FundGoalTrendsSummaryCardsProps {
  readonly progress: readonly FundGoalPeriodProgress[];
}

/**
 * Displays top-level Fund Goal health for the selected trends range.
 */
const FundGoalTrendsSummaryCards = function ({
  progress,
}: FundGoalTrendsSummaryCardsProps): JSX.Element {
  const summary = getFundGoalHealthSummary(progress);
  const achievedPercentage =
    summary.configuredGoalCount === 0
      ? 0
      : (summary.satisfiedGoalCount / summary.configuredGoalCount) * 100;

  return (
    <ResponsiveGrid columns={{ xs: 1, md: 2 }}>
      <SummaryCard title="Fund Goals Achieved">
        <Stack spacing={2}>
          <LabeledAmountBar
            label="Achieved Fund Goals"
            value={`${achievedPercentage.toFixed(0)}% (${summary.satisfiedGoalCount} of ${summary.configuredGoalCount})`}
            ratio={achievedPercentage / 100}
            color="success.main"
          />
        </Stack>
      </SummaryCard>
      <SummaryCard title="Expected Fund Goal Contributions vs. Actual">
        <Stack spacing={2}>
          <ComparisonBarPair
            first={{
              label: "Expected contributions",
              amount: summary.targetContribution,
              color: "info.main",
              differenceLabel: "Surplus",
              differenceColor: "success.main",
            }}
            second={{
              label: "Assigned contributions",
              amount: summary.assignedContribution,
              color: "success.main",
              differenceLabel: "Shortfall",
              differenceColor: "error.main",
            }}
          />
        </Stack>
      </SummaryCard>
    </ResponsiveGrid>
  );
};

export default FundGoalTrendsSummaryCards;
