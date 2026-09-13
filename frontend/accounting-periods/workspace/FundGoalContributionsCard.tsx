"use client";

import { Box, Divider, Stack, Typography } from "@mui/material";
import ComparisonBar from "@/framework/view/ComparisonBar";
import ComparisonBarPair from "@/framework/view/ComparisonBarPair";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the FundGoalContributionsCard component.
 */
interface FundGoalContributionsCardProps {
  readonly trackedIncome: number;
  readonly regularContributions: number;
  readonly extraContributions: number;
  readonly expectedRegularContributions: number;
}

/**
 * Displays actual Fund Goal contributions as regular and extra segments.
 */
const ContributionBreakdownBar = function ({
  regular,
  extra,
  maxAmount,
}: {
  readonly regular: number;
  readonly extra: number;
  readonly maxAmount: number;
}): JSX.Element {
  const regularRatio = regular / maxAmount;
  const extraRatio = extra / maxAmount;

  return (
    <Stack spacing={0.75}>
      <Stack
        direction="row"
        justifyContent="space-between"
        flexWrap="wrap"
        useFlexGap
      >
        <Typography
          variant="body2"
          color="success.main"
          fontWeight={600}
          noWrap
        >
          Regular: {formatCurrency(regular)}
        </Typography>
        <Typography
          variant="body2"
          color="warning.main"
          fontWeight={600}
          noWrap
        >
          Extra: {formatCurrency(extra)}
        </Typography>
      </Stack>
      <Box
        role="img"
        aria-label={`Fund Goal contributions, regular ${formatCurrency(regular)}, extra ${formatCurrency(extra)}`}
        sx={{
          backgroundColor: "divider",
          borderRadius: 1,
          display: "flex",
          height: 16,
          overflow: "hidden",
          width: "100%",
        }}
      >
        <Box
          sx={{
            backgroundColor: "success.main",
            height: "100%",
            width: `${Math.round(regularRatio * 100)}%`,
          }}
        />
        <Box
          sx={{
            backgroundColor: "warning.main",
            height: "100%",
            width: `${Math.round(extraRatio * 100)}%`,
          }}
        />
      </Box>
    </Stack>
  );
};

/**
 * Relates tracked income to actual Fund Goal funding and regular contribution expectations.
 */
const FundGoalContributionsCard = function ({
  trackedIncome,
  regularContributions,
  extraContributions,
  expectedRegularContributions,
}: FundGoalContributionsCardProps): JSX.Element {
  const maxAmount = Math.max(
    trackedIncome,
    regularContributions + extraContributions,
    1,
  );

  return (
    <SummaryCard title="Fund Goal Contributions">
      <Stack spacing={2}>
        <ComparisonBar
          label="Tracked Income"
          amount={trackedIncome}
          amountColor="success.main"
          maxAmount={maxAmount}
        />
        <ContributionBreakdownBar
          regular={regularContributions}
          extra={extraContributions}
          maxAmount={maxAmount}
        />
        <Divider />
        <ComparisonBarPair
          first={{
            label: "Regular",
            amount: regularContributions,
            color: "success.main",
            differenceLabel: "Shortfall",
            differenceColor: "error.main",
          }}
          second={{
            label: "Expected Regular",
            amount: expectedRegularContributions,
            color: "info.main",
            differenceLabel: "Surplus",
            differenceColor: "success.main",
          }}
        />
      </Stack>
    </SummaryCard>
  );
};

export type { FundGoalContributionsCardProps };
export default FundGoalContributionsCard;
