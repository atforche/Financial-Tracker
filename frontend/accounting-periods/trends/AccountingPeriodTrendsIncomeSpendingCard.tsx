"use client";

import { Box, Stack, Typography } from "@mui/material";
import type { AccountingPeriodTrends } from "@/accounting-periods/types";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

interface AccountingPeriodTrendsIncomeSpendingCardProps {
  readonly trends: AccountingPeriodTrends;
}

/**
 * Component that displays total income and spending for the Accounting Periods trends.
 */
const AccountingPeriodTrendsIncomeSpendingCard = function ({
  trends,
}: AccountingPeriodTrendsIncomeSpendingCardProps): JSX.Element {
  const maxAmount = Math.max(trends.totalIncome, trends.totalSpending, 1);
  const incomeRatio = trends.totalIncome / maxAmount;
  const spendingRatio = trends.totalSpending / maxAmount;

  return (
    <SummaryCard title="Income vs. spending">
      <Stack spacing={2}>
        <Stack
          direction="row"
          justifyContent="space-between"
          alignItems="center"
        >
          <Typography color="text.secondary">Total income</Typography>
          <Typography fontWeight={600} color="success.main">
            {formatCurrency(trends.totalIncome)}
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
              width: `${Math.round(incomeRatio * 100)}%`,
              height: "100%",
              backgroundColor: "success.main",
              transition: "width 0.2s ease",
            }}
          />
        </Box>
        <Stack
          direction="row"
          justifyContent="space-between"
          alignItems="center"
        >
          <Typography color="text.secondary">Total spending</Typography>
          <Typography fontWeight={600} color="error.main">
            {formatCurrency(trends.totalSpending)}
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
              width: `${Math.round(spendingRatio * 100)}%`,
              height: "100%",
              backgroundColor: "error.main",
              transition: "width 0.2s ease",
            }}
          />
        </Box>
      </Stack>
    </SummaryCard>
  );
};

export default AccountingPeriodTrendsIncomeSpendingCard;
