"use client";

import type { JSX } from "react";
import LabeledAmountBar from "@/framework/view/LabeledAmountBar";
import { Stack } from "@mui/material";
import SummaryCard from "@/framework/view/SummaryCard";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the FundTrendsAssignmentSpendingCard component.
 */
interface FundTrendsAssignmentSpendingCardProps {
  readonly totalAssigned: number;
  readonly totalSpent: number;
}

/**
 * Component that displays total assigned and total spent for Funds.
 */
const FundTrendsAssignmentSpendingCard = function ({
  totalAssigned,
  totalSpent,
}: FundTrendsAssignmentSpendingCardProps): JSX.Element {
  const maxAmount = Math.max(totalAssigned, totalSpent, 1);
  const assignedRatio = totalAssigned / maxAmount;
  const spentRatio = totalSpent / maxAmount;

  return (
    <SummaryCard title="Assignment vs. spending">
      <Stack spacing={2}>
        <LabeledAmountBar
          label="Total assigned"
          value={formatCurrency(totalAssigned)}
          ratio={assignedRatio}
          color="success.main"
        />
        <LabeledAmountBar
          label="Total spent"
          value={formatCurrency(totalSpent)}
          ratio={spentRatio}
          color="error.main"
        />
      </Stack>
    </SummaryCard>
  );
};

export default FundTrendsAssignmentSpendingCard;
