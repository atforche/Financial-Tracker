"use client";

import { Box, Stack } from "@mui/material";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

interface AccountingPeriodTrendsSummaryCardsProps {
  readonly accountingPeriods: AccountingPeriodWithBalance[];
}

/**
 * Summary metrics derived from the selected accounting period trends range.
 */
interface TrendsSnapshot {
  readonly startLabel: string;
  readonly endLabel: string;
  readonly totalStartingBalance: number;
  readonly totalEndingBalance: number;
}

const getTrendsSnapshot = function (
  accountingPeriods: AccountingPeriodWithBalance[],
): TrendsSnapshot {
  const periods = accountingPeriods;
  const firstPeriod = periods.at(0);
  const lastPeriod = periods.at(-1);

  if (typeof firstPeriod === "undefined" || typeof lastPeriod === "undefined") {
    return {
      startLabel: "Start",
      endLabel: "End",
      totalStartingBalance: 0,
      totalEndingBalance: 0,
    };
  }
  return {
    startLabel: firstPeriod.name,
    endLabel: lastPeriod.name,
    totalStartingBalance: firstPeriod.openingBalance,
    totalEndingBalance: lastPeriod.closingBalance,
  };
};

/**
 * Displays the top-level accounting period balance summary cards.
 */
const AccountingPeriodTrendsSummaryCards = function ({
  accountingPeriods,
}: AccountingPeriodTrendsSummaryCardsProps): JSX.Element {
  const snapshot = getTrendsSnapshot(accountingPeriods);
  const netChange = snapshot.totalEndingBalance - snapshot.totalStartingBalance;
  const percentChange =
    snapshot.totalStartingBalance === 0
      ? 0
      : (netChange / Math.abs(snapshot.totalStartingBalance)) * 100;
  const isPositive = netChange >= 0;
  const valueColor = isPositive ? "success.main" : "error.main";

  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        gridTemplateColumns: {
          xs: "1fr",
          md: "repeat(3, minmax(0, 1fr))",
        },
      }}
    >
      <SummaryCard
        title={`Starting balance (${snapshot.startLabel})`}
        value={<Stack>{formatCurrency(snapshot.totalStartingBalance)}</Stack>}
      />
      <SummaryCard
        title={`Ending balance (${snapshot.endLabel})`}
        value={<Stack>{formatCurrency(snapshot.totalEndingBalance)}</Stack>}
      />
      <SummaryCard
        title="Net change"
        value={
          <Stack>
            <Box component="span" sx={{ color: valueColor }}>
              {formatCurrency(netChange)} ({isPositive ? "+" : ""}
              {percentChange.toFixed(2)}%)
            </Box>
          </Stack>
        }
      />
    </Box>
  );
};

export default AccountingPeriodTrendsSummaryCards;
