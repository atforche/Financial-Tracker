"use client";

import { Box, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import type { TransactionSummaryByType } from "@/transactions/transaction";
import formatCurrency from "@/framework/formatCurrency";

interface TransactionTrendsByTypeCardProps {
  readonly transactionTypes: TransactionSummaryByType[];
}

/**
 * Component that displays transaction count and amount broken down by type.
 */
const TransactionTrendsByTypeCard = function ({
  transactionTypes,
}: TransactionTrendsByTypeCardProps): JSX.Element {
  const maxTransactionCount = Math.max(
    ...transactionTypes.map((summary) => summary.totalCount),
    1,
  );
  const maxTransactionAmount = Math.max(
    ...transactionTypes.map((summary) => summary.totalAmount),
    1,
  );

  const renderTypeSummary = function (
    summary: TransactionSummaryByType,
  ): JSX.Element {
    const transactionCountRatio = summary.totalCount / maxTransactionCount;
    const transactionAmountRatio = summary.totalAmount / maxTransactionAmount;

    return (
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
          <Stack spacing={0.5}>
            <Stack
              direction="row"
              justifyContent="space-between"
              alignItems="center"
            >
              <Typography variant="body2" color="text.secondary">
                Count
              </Typography>
              <Typography variant="body2" fontWeight={600} color="primary.main">
                {summary.totalCount.toLocaleString()}
              </Typography>
            </Stack>
            <Box
              sx={{
                width: "100%",
                height: 10,
                borderRadius: 999,
                backgroundColor: "divider",
                overflow: "hidden",
              }}
            >
              <Box
                sx={{
                  width: `${Math.round(transactionCountRatio * 100)}%`,
                  height: "100%",
                  backgroundColor: "primary.main",
                  transition: "width 0.2s ease",
                }}
              />
            </Box>
          </Stack>
          <Stack spacing={0.5}>
            <Stack
              direction="row"
              justifyContent="space-between"
              alignItems="center"
            >
              <Typography variant="body2" color="text.secondary">
                Amount
              </Typography>
              <Typography variant="body2" fontWeight={600} color="info.main">
                {formatCurrency(summary.totalAmount)}
              </Typography>
            </Stack>
            <Box
              sx={{
                width: "100%",
                height: 10,
                borderRadius: 999,
                backgroundColor: "divider",
                overflow: "hidden",
              }}
            >
              <Box
                sx={{
                  width: `${Math.round(transactionAmountRatio * 100)}%`,
                  height: "100%",
                  backgroundColor: "info.main",
                  transition: "width 0.2s ease",
                }}
              />
            </Box>
          </Stack>
        </Stack>
      </Stack>
    );
  };

  if (transactionTypes.length === 0) {
    return (
      <SummaryCard title="Transactions by type">
        <Typography color="text.secondary">
          No transaction type summaries are available for the selected range.
        </Typography>
      </SummaryCard>
    );
  }

  return (
    <SummaryCard title="Transactions by type">
      <Stack spacing={2.5}>
        {transactionTypes.map(renderTypeSummary)}
      </Stack>
    </SummaryCard>
  );
};

export default TransactionTrendsByTypeCard;
