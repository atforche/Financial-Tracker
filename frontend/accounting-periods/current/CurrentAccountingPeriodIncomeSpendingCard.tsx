"use client";

import { Box, Stack, Typography } from "@mui/material";
import type { CurrentAccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

interface AccountingPeriodCurrentIncomeSpendingCardProps {
  readonly current: CurrentAccountingPeriod;
}

interface AmountBarProps {
  readonly ratio: number;
  readonly color: string;
  readonly height?: number;
}

interface IncomeBreakdownRowProps {
  readonly label: string;
  readonly amount: number;
  readonly ratio: number;
  readonly color: string;
}

const AmountBar = function ({
  ratio,
  color,
  height = 16,
}: AmountBarProps): JSX.Element {
  return (
    <Box
      sx={{
        width: "100%",
        height,
        borderRadius: 1,
        backgroundColor: "divider",
        overflow: "hidden",
      }}
    >
      <Box
        sx={{
          width: `${Math.round(ratio * 100)}%`,
          height: "100%",
          backgroundColor: color,
          transition: "width 0.2s ease",
        }}
      />
    </Box>
  );
};

const IncomeBreakdownRow = function ({
  label,
  amount,
  ratio,
  color,
}: IncomeBreakdownRowProps): JSX.Element {
  return (
    <Stack spacing={1}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant="body2" color="text.secondary">
          {label}
        </Typography>
        <Typography variant="body2" fontWeight={600} color={color}>
          {formatCurrency(amount)}
        </Typography>
      </Stack>
      <AmountBar ratio={ratio} color={color} height={12} />
    </Stack>
  );
};

/**
 * Component that displays total income and spending for the current Accounting Period.
 */
const CurrentAccountingPeriodIncomeSpendingCard = function ({
  current,
}: AccountingPeriodCurrentIncomeSpendingCardProps): JSX.Element {
  const totalIncome = current.totalIncome.total;
  const trackedIncome = current.totalIncome.tracked;
  const untrackedIncome = current.totalIncome.untracked;
  const maxAmount = Math.max(totalIncome, current.totalSpending, 1);
  const incomeRatio = totalIncome / maxAmount;
  const spendingRatio = current.totalSpending / maxAmount;
  const trackedIncomeRatio =
    totalIncome === 0 ? 0 : trackedIncome / totalIncome;
  const untrackedIncomeRatio =
    totalIncome === 0 ? 0 : untrackedIncome / totalIncome;

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
            {formatCurrency(totalIncome)}
          </Typography>
        </Stack>
        <AmountBar ratio={incomeRatio} color="success.main" />
        <Stack spacing={1.5} sx={{ pl: 2 }}>
          <IncomeBreakdownRow
            label="Tracked income"
            amount={trackedIncome}
            ratio={trackedIncomeRatio}
            color="success.main"
          />
          <IncomeBreakdownRow
            label="Untracked income"
            amount={untrackedIncome}
            ratio={untrackedIncomeRatio}
            color="success.main"
          />
        </Stack>
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
        <AmountBar ratio={spendingRatio} color="error.main" />
      </Stack>
    </SummaryCard>
  );
};

export default CurrentAccountingPeriodIncomeSpendingCard;
