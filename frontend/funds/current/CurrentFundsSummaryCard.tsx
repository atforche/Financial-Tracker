"use client";

import { Box, Divider, Paper, Stack, Typography } from "@mui/material";
import type { CurrentFunds } from "@/funds/types";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface CurrentFundsSummaryCardProps {
  readonly current: CurrentFunds;
}

/**
 * Displays the current aggregate fund balance with the visible assigned breakdown.
 */
const CurrentFundsSummaryCard = function ({
  current,
}: CurrentFundsSummaryCardProps): JSX.Element {
  const description =
    current.funds.length === 0
      ? "Add a fund to start building this snapshot."
      : null;

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: { xs: 2, md: 2.5 },
        background:
          "linear-gradient(180deg, rgba(16,185,129,0.08) 0%, rgba(255,255,255,0.98) 100%)",
        boxShadow: "0 8px 24px rgba(15, 23, 42, 0.04)",
      }}
    >
      <Stack spacing={2}>
        <Stack spacing={0.75}>
          <Typography
            variant="overline"
            sx={{
              color: "text.secondary",
              fontWeight: 700,
              letterSpacing: 1.1,
            }}
          >
            Current total balance
          </Typography>
          <Typography variant="h4">
            {formatCurrency(current.summary.totalTrackedBalance)}
          </Typography>
          {description === null ? null : (
            <Typography variant="body2" color="text.secondary">
              {description}
            </Typography>
          )}
        </Stack>
        <Box
          sx={{
            border: "1px solid",
            borderColor: "divider",
            borderRadius: 2,
            px: 1.5,
            py: 1.25,
            backgroundColor: "rgba(248, 250, 252, 0.9)",
          }}
        >
          <Stack spacing={1.25}>
            <Typography
              variant="body2"
              sx={{ color: "text.secondary", fontWeight: 600 }}
            >
              Balance breakdown
            </Typography>
            <Stack divider={<Divider flexItem />}>
              <Stack
                direction="row"
                justifyContent="space-between"
                alignItems="center"
                gap={2}
                sx={{ py: 0.65 }}
              >
                <Typography variant="body2" color="text.secondary">
                  Assigned funds
                </Typography>
                <Typography variant="body2" fontWeight={700}>
                  {formatCurrency(current.summary.totalAssignedBalance)}
                </Typography>
              </Stack>
              <Stack
                direction="row"
                justifyContent="space-between"
                alignItems="center"
                gap={2}
                sx={{ py: 0.65 }}
              >
                <Typography variant="body2" color="text.secondary">
                  Unassigned fund
                </Typography>
                <Typography variant="body2" fontWeight={700}>
                  {formatCurrency(current.summary.totalUnassignedBalance)}
                </Typography>
              </Stack>
            </Stack>
          </Stack>
        </Box>
      </Stack>
    </Paper>
  );
};

export default CurrentFundsSummaryCard;
