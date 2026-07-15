"use client";

import { Box, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

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
        <Stack
          direction="row"
          justifyContent="space-between"
          alignItems="center"
        >
          <Typography color="text.secondary">Total assigned</Typography>
          <Typography fontWeight={600} color="success.main">
            {formatCurrency(totalAssigned)}
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
              width: `${Math.round(assignedRatio * 100)}%`,
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
          <Typography color="text.secondary">Total spent</Typography>
          <Typography fontWeight={600} color="error.main">
            {formatCurrency(totalSpent)}
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
              width: `${Math.round(spentRatio * 100)}%`,
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

export default FundTrendsAssignmentSpendingCard;
