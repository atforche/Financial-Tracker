"use client";

import { Box, Stack, Typography } from "@mui/material";
import type { CurrentAccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

interface AccountingPeriodCurrentIncomeSpendingCardProps {
  readonly current: CurrentAccountingPeriod;
}

/**
 * Component that displays total income and spending for the current Accounting Period.
 */
const CurrentAccountingPeriodIncomeSpendingCard = function ({
  current,
}: AccountingPeriodCurrentIncomeSpendingCardProps): JSX.Element {
  const maxAmount = Math.max(current.totalIncome, current.totalSpending, 1);
  const incomeRatio = current.totalIncome / maxAmount;
  const spendingRatio = current.totalSpending / maxAmount;

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
            {formatCurrency(current.totalIncome)}
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
            {formatCurrency(current.totalSpending)}
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

export default CurrentAccountingPeriodIncomeSpendingCard;
