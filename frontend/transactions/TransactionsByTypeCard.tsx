"use client";

import { Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import LabeledAmountBar from "@/framework/view/LabeledAmountBar";
import SummaryCard from "@/framework/view/SummaryCard";
import type { TransactionSummaryByType } from "@/transactions/types";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for TransactionsByTypeCard component.
 */
interface TransactionsByTypeCardProps {
  readonly transactionTypes: TransactionSummaryByType[];
}

/**
 * Displays transaction count and amount broken down by type.
 */
const TransactionsByTypeCard = function ({
  transactionTypes,
}: TransactionsByTypeCardProps): JSX.Element {
  const maxTransactionCount = Math.max(
    ...transactionTypes.map((summary) => summary.totalCount),
    1,
  );
  const maxTransactionAmount = Math.max(
    ...transactionTypes.map((summary) => summary.totalAmount),
    1,
  );

  if (transactionTypes.length === 0) {
    return (
      <SummaryCard title="Transactions by type">
        <Typography color="text.secondary">No transaction type summaries are available.</Typography>
      </SummaryCard>
    );
  }

  return (
    <SummaryCard title="Transactions by type">
      <Stack spacing={2.5}>
        {transactionTypes.map((summary) => (
          <Stack key={summary.transactionType} spacing={1.25}>
            <Stack
              direction="row"
              justifyContent="space-between"
              alignItems="center"
            >
              <Typography fontWeight={600}>{summary.transactionType}</Typography>
              <Typography color="text.secondary">
                {summary.totalCount.toLocaleString()} transactions
              </Typography>
            </Stack>
            <Stack spacing={1}>
              <LabeledAmountBar
                label="Count"
                value={summary.totalCount.toLocaleString()}
                ratio={summary.totalCount / maxTransactionCount}
                color="primary.main"
                barHeight={10}
                barRadius={999}
                compact
              />
              <LabeledAmountBar
                label="Amount"
                value={formatCurrency(summary.totalAmount)}
                ratio={summary.totalAmount / maxTransactionAmount}
                color="info.main"
                barHeight={10}
                barRadius={999}
                compact
              />
            </Stack>
          </Stack>
        ))}
      </Stack>
    </SummaryCard>
  );
};

export type { TransactionsByTypeCardProps };
export default TransactionsByTypeCard;
