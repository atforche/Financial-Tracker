"use client";

import { Box, Stack } from "@mui/material";
import type { AccountingPeriodDashboard } from "@/accounting-periods/types";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

interface AccountingPeriodDashboardSummaryCardsProps {
  readonly dashboard: AccountingPeriodDashboard;
}

/**
 * Summary metrics derived from the selected accounting period dashboard range.
 */
interface DashboardSnapshot {
  readonly startLabel: string;
  readonly endLabel: string;
  readonly totalStartingBalance: number;
  readonly totalEndingBalance: number;
}

const getDashboardSnapshot = function (
  dashboard: AccountingPeriodDashboard,
): DashboardSnapshot {
  const periods = dashboard.accountingPeriods.items;
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
const AccountingPeriodDashboardSummaryCards = function ({
  dashboard,
}: AccountingPeriodDashboardSummaryCardsProps): JSX.Element {
  const snapshot = getDashboardSnapshot(dashboard);
  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        gridTemplateColumns: {
          xs: "1fr",
          md: "repeat(2, minmax(0, 1fr))",
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
    </Box>
  );
};

export default AccountingPeriodDashboardSummaryCards;
