"use client";

import { Box, Stack } from "@mui/material";
import type { AccountingPeriodWithTransactions } from "@/accounting-periods/types";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

interface AccountingPeriodCurrentSummaryCardsProps {
  readonly current: AccountingPeriodWithTransactions | null;
}

/**
 * Displays the top-level current accounting period balance summary cards.
 */
const CurrentAccountingPeriodSummaryCards = function ({
  current,
}: AccountingPeriodCurrentSummaryCardsProps): JSX.Element {
  const openingBalance = current?.openingBalance ?? 0;
  const closingBalance = current?.closingBalance ?? 0;
  const netChange = closingBalance - openingBalance;
  const percentChange =
    openingBalance === 0 ? 0 : (netChange / Math.abs(openingBalance)) * 100;
  const isPositive = netChange >= 0;
  const valueColor = isPositive ? "success.main" : "error.main";
  const titleSuffix = current === null ? "" : ` (${current.name})`;

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
        title={`Opening balance${titleSuffix}`}
        value={<Stack>{formatCurrency(openingBalance)}</Stack>}
      />
      <SummaryCard
        title={`Closing balance${titleSuffix}`}
        value={<Stack>{formatCurrency(closingBalance)}</Stack>}
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

export default CurrentAccountingPeriodSummaryCards;
