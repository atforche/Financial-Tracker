"use client";

import { Box, Stack, Typography } from "@mui/material";
import type { AccountingPeriodDashboard } from "@/accounting-periods/types";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

interface AccountingPeriodDashboardIncomeSpendingCardProps {
  readonly dashboard: AccountingPeriodDashboard;
}

/**
 * Component that displays total income and spending for the Accounting Periods dashboard.
 */
const AccountingPeriodDashboardIncomeSpendingCard = function ({
  dashboard,
}: AccountingPeriodDashboardIncomeSpendingCardProps): JSX.Element {
  const maxAmount = Math.max(dashboard.totalIncome, dashboard.totalSpending, 1);
  const incomeRatio = dashboard.totalIncome / maxAmount;
  const spendingRatio = dashboard.totalSpending / maxAmount;

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
            {formatCurrency(dashboard.totalIncome)}
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
            {formatCurrency(dashboard.totalSpending)}
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

export default AccountingPeriodDashboardIncomeSpendingCard;
