"use client";

import { Box, Stack, Typography } from "@mui/material";
import ComparisonBar from "@/framework/view/ComparisonBar";
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
 * Compares actual regular contributions with their expected value on one bar.
 */
const RegularContributionComparisonBar = function ({
  actual,
  expected,
  extra,
  maxAmount,
}: {
  readonly actual: number;
  readonly expected: number;
  readonly extra: number;
  readonly maxAmount: number;
}): JSX.Element {
  const difference = actual - expected;
  const comparisonAmount = Math.max(Math.min(actual, expected), 0);
  const comparisonRatio = comparisonAmount / maxAmount;
  const differenceRatio = Math.abs(difference) / maxAmount;
  const extraRatio = extra / maxAmount;
  const differenceLabel = difference >= 0 ? "Surplus" : "Shortcoming";
  const comparisonCaption =
    difference === 0
      ? "On target"
      : `${differenceLabel}: ${formatCurrency(Math.abs(difference))}`;

  return (
    <Stack spacing={0.75}>
      <Stack
        direction="row"
        justifyContent="space-between"
        spacing={1}
        flexWrap="wrap"
        useFlexGap
      >
        <Typography variant="body2" color="info.main" fontWeight={600} noWrap>
          Expected Regular: {formatCurrency(expected)}
        </Typography>
        <Typography
          variant="body2"
          fontWeight={600}
          color={
            difference === 0
              ? "text.secondary"
              : difference > 0
                ? "success.main"
                : "error.main"
          }
          textAlign="right"
          noWrap
        >
          {comparisonCaption}
        </Typography>
      </Stack>
      <Box
        role="img"
        aria-label={`Expected regular contributions ${formatCurrency(expected)}, actual ${formatCurrency(actual)}, extra ${formatCurrency(extra)}, ${comparisonCaption}`}
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
            backgroundColor: "info.main",
            height: "100%",
            width: `${Math.round(comparisonRatio * 100)}%`,
          }}
        />
        {difference !== 0 && (
          <Box
            sx={{
              backgroundColor: difference > 0 ? "success.main" : "error.main",
              height: "100%",
              width: `${Math.round(differenceRatio * 100)}%`,
            }}
          />
        )}
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
    expectedRegularContributions + extraContributions,
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
        <RegularContributionComparisonBar
          actual={regularContributions}
          expected={expectedRegularContributions}
          extra={extraContributions}
          maxAmount={maxAmount}
        />
      </Stack>
    </SummaryCard>
  );
};

export type { FundGoalContributionsCardProps };
export default FundGoalContributionsCard;
