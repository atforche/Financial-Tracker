"use client";

import { Box, Stack, Typography } from "@mui/material";
import {
  type FundGoalPeriodProgress,
  getFundGoalHealthSummary,
} from "@/fund-goals/trends/fundGoalProgressTrends";
import ComparisonBarPair from "@/framework/view/ComparisonBarPair";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
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
  const achievedRatio = Math.min(Math.max(achievedPercentage / 100, 0), 1);

  return (
    <ResponsiveGrid columns={{ xs: 1, md: 2 }}>
      <SummaryCard title="Fund Goals Achieved">
        <Stack spacing={2}>
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Typography color="text.secondary">Achieved Fund Goals</Typography>
            <Typography fontWeight={600} color="success.main">
              {achievedPercentage.toFixed(0)}% ({summary.satisfiedGoalCount} of{" "}
              {summary.configuredGoalCount})
            </Typography>
          </Stack>
          <Box
            sx={{
              width: "100%",
              height: 16,
              borderRadius: 1,
              backgroundColor: "divider",
              overflow: "hidden",
            }}
          >
            <Box
              sx={{
                width: `${Math.round(achievedRatio * 100)}%`,
                height: "100%",
                backgroundColor: "success.main",
                transition: "width 0.2s ease",
              }}
            />
          </Box>
        </Stack>
      </SummaryCard>
      <SummaryCard title="Expected Fund Goal Contributions vs. Actual">
        <Stack spacing={2}>
          <ComparisonBarPair
            first={{
              label: "Expected contributions",
              amount: summary.expectedContribution,
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
