"use client";

import { Box, Stack } from "@mui/material";
import ComparisonBarPair from "@/framework/view/ComparisonBarPair";
import FundGoalContributionsCard from "@/accounting-periods/workspace/FundGoalContributionsCard";
import type { IncomeAmount } from "@/transactions/types";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";

const emptyIncome: IncomeAmount = {
  total: 0,
  tracked: 0,
  untracked: 0,
};

/**
 * Props for the AccountingPeriodProgressFlow component.
 */
interface AccountingPeriodProgressFlowProps {
  readonly expectedIncome: IncomeAmount | undefined;
  readonly actualIncome: IncomeAmount | undefined;
  readonly totalSpending: number | undefined;
  readonly expectedFundGoalContributions: number | undefined;
  readonly actualFundGoalContributions: number | undefined;
  readonly actualExtraFundGoalContributions: number | undefined;
}

/**
 * Returns a finite financial amount, defaulting missing values to zero.
 */
const getAmount = (amount: number | undefined): number =>
  typeof amount === "number" && Number.isFinite(amount) ? amount : 0;

/**
 * Displays expected and actual values for one income category.
 */
const IncomeComparisonCard = function ({
  title,
  expected,
  actual,
}: {
  readonly title: string;
  readonly expected: number;
  readonly actual: number;
}): JSX.Element {
  return (
    <SummaryCard title={title}>
      <ComparisonBarPair
        first={{
          label: "Actual",
          amount: actual,
          color: "success.main",
          differenceLabel: "Shortfall",
          differenceColor: "error.main",
        }}
        second={{
          label: "Expected",
          amount: expected,
          color: "info.main",
          differenceLabel: "Surplus",
          differenceColor: "success.main",
        }}
      />
    </SummaryCard>
  );
};

/**
 * Draws the income story for one accounting period as a hierarchy of cards.
 */
const AccountingPeriodProgressFlow = function ({
  expectedIncome = emptyIncome,
  actualIncome = emptyIncome,
  totalSpending,
  expectedFundGoalContributions,
  actualFundGoalContributions,
  actualExtraFundGoalContributions,
}: AccountingPeriodProgressFlowProps): JSX.Element {
  return (
    <Stack
      component="section"
      spacing={0}
      aria-label="Income and spending progress"
      sx={{ containerType: "inline-size" }}
    >
      <Box
        sx={{
          alignSelf: "flex-start",
          maxWidth: "none",
          width: "100%",
          "@container (min-width: 760px)": {
            alignSelf: "center",
            width: "66.666%",
          },
        }}
      >
        <IncomeComparisonCard
          title="Total Income"
          expected={getAmount(expectedIncome.total)}
          actual={getAmount(actualIncome.total)}
        />
      </Box>
      <Box
        aria-hidden="true"
        sx={{
          alignSelf: "center",
          borderColor: "divider",
          borderLeftStyle: "solid",
          borderLeftWidth: 2,
          display: "none",
          height: 24,
          "@container (min-width: 760px)": { display: "block" },
        }}
      />
      <Box
        aria-hidden="true"
        sx={{
          borderColor: "divider",
          borderLeftStyle: "solid",
          borderLeftWidth: 2,
          borderRightStyle: "solid",
          borderRightWidth: 2,
          borderTopStyle: "solid",
          borderTopWidth: 2,
          display: "none",
          height: 24,
          mx: "25%",
          "@container (min-width: 760px)": { display: "block" },
        }}
      />
      <Box
        sx={{
          display: "grid",
          gap: 3,
          alignItems: "start",
          borderColor: "divider",
          borderLeftStyle: "solid",
          borderLeftWidth: 2,
          gridTemplateColumns: "minmax(0, 1fr)",
          ml: 2,
          mt: 3,
          pl: 3,
          "& > *": { position: "relative" },
          "& > *::before": {
            borderColor: "divider",
            borderTopStyle: "solid",
            borderTopWidth: 2,
            content: '""',
            left: -24,
            position: "absolute",
            top: 32,
            width: 24,
          },
          "& > *:last-child::after": {
            backgroundColor: "background.default",
            bottom: 0,
            content: '""',
            left: -26,
            position: "absolute",
            top: 34,
            width: 2,
          },
          "@container (min-width: 760px)": {
            borderLeftWidth: 0,
            gridTemplateColumns: "minmax(0, 2fr) minmax(0, 1fr)",
            ml: 0,
            mt: 0,
            pl: 0,
            "& > *::before": { display: "none" },
            "& > *::after": { display: "none" },
          },
        }}
      >
        <Box>
          <Stack spacing={0}>
            <IncomeComparisonCard
              title="Tracked Income"
              expected={getAmount(expectedIncome.tracked)}
              actual={getAmount(actualIncome.tracked)}
            />
            <Box
              aria-hidden="true"
              sx={{
                alignSelf: "center",
                borderColor: "divider",
                borderLeftStyle: "solid",
                borderLeftWidth: 2,
                display: "none",
                height: 24,
                "@container (min-width: 1180px)": { display: "block" },
              }}
            />
            <Box
              aria-hidden="true"
              sx={{
                borderColor: "divider",
                borderLeftStyle: "solid",
                borderLeftWidth: 2,
                borderRightStyle: "solid",
                borderRightWidth: 2,
                borderTopStyle: "solid",
                borderTopWidth: 2,
                display: "none",
                height: 24,
                mx: "25%",
                "@container (min-width: 1180px)": { display: "block" },
              }}
            />
            <Box
              sx={{
                display: "grid",
                gap: 2,
                borderColor: "divider",
                borderLeftStyle: "solid",
                borderLeftWidth: 2,
                gridTemplateColumns: "minmax(0, 1fr)",
                ml: 2,
                mt: 2,
                pl: 3,
                "& > *": { position: "relative" },
                "& > *::before": {
                  borderColor: "divider",
                  borderTopStyle: "solid",
                  borderTopWidth: 2,
                  content: '""',
                  left: -24,
                  position: "absolute",
                  top: 32,
                  width: 24,
                },
                "& > *:last-child::after": {
                  backgroundColor: "background.default",
                  bottom: 0,
                  content: '""',
                  left: -26,
                  position: "absolute",
                  top: 34,
                  width: 2,
                },
                "@container (min-width: 1180px)": {
                  borderLeftWidth: 0,
                  gridTemplateColumns: "repeat(2, minmax(0, 1fr))",
                  ml: 0,
                  mt: 0,
                  pl: 0,
                  "& > *::before": { display: "none" },
                  "& > *::after": { display: "none" },
                },
              }}
            >
              <Box>
                <SummaryCard title="Tracked Income vs. Spending">
                  <ComparisonBarPair
                    first={{
                      label: "Tracked Income",
                      amount: getAmount(actualIncome.tracked),
                      color: "success.main",
                      differenceLabel: "Shortfall",
                      differenceColor: "error.main",
                    }}
                    second={{
                      label: "Spending",
                      amount: getAmount(totalSpending),
                      color: "error.main",
                      differenceLabel: "Remaining",
                      differenceColor: "success.main",
                    }}
                  />
                </SummaryCard>
              </Box>
              <Box>
                <FundGoalContributionsCard
                  trackedIncome={getAmount(actualIncome.tracked)}
                  regularContributions={getAmount(actualFundGoalContributions)}
                  extraContributions={getAmount(
                    actualExtraFundGoalContributions,
                  )}
                  expectedRegularContributions={getAmount(
                    expectedFundGoalContributions,
                  )}
                />
              </Box>
            </Box>
          </Stack>
        </Box>
        <Box>
          <IncomeComparisonCard
            title="Untracked Income"
            expected={getAmount(expectedIncome.untracked)}
            actual={getAmount(actualIncome.untracked)}
          />
        </Box>
      </Box>
    </Stack>
  );
};

export type { AccountingPeriodProgressFlowProps };
export default AccountingPeriodProgressFlow;
