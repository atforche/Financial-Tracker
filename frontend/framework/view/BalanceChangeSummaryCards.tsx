"use client";

import { Box, Stack } from "@mui/material";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the BalanceChangeSummaryCards component.
 */
interface BalanceChangeSummaryCardsProps {
  readonly startingTitle: string;
  readonly endingTitle: string;
  readonly startingBalance: number;
  readonly endingBalance: number;
}

/**
 * Displays starting, ending, and net balance change summary cards.
 */
const BalanceChangeSummaryCards = function ({
  startingTitle,
  endingTitle,
  startingBalance,
  endingBalance,
}: BalanceChangeSummaryCardsProps): JSX.Element {
  const netChange = endingBalance - startingBalance;
  const percentChange =
    startingBalance === 0 ? 0 : (netChange / Math.abs(startingBalance)) * 100;
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
        title={startingTitle}
        value={<Stack>{formatCurrency(startingBalance)}</Stack>}
      />
      <SummaryCard
        title={endingTitle}
        value={<Stack>{formatCurrency(endingBalance)}</Stack>}
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

export default BalanceChangeSummaryCards;
