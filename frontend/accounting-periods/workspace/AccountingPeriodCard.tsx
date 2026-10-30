"use client";

import { Box, Chip, Paper, Stack, Typography } from "@mui/material";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import type { JSX } from "react";
import Link from "next/link";
import SignedChange from "@/framework/view/SignedChange";
import routes from "@/accounting-periods/routes";

/**
 * Props for the AccountingPeriodCard component.
 */
interface AccountingPeriodCardProps {
  readonly period: AccountingPeriodWithBalance;
}

/**
 * Summarizes an accounting period, linking to it: its closing net worth and change over the period, and how much of
 * its expected tracked income has been received.
 */
const AccountingPeriodCard = function ({
  period,
}: AccountingPeriodCardProps): JSX.Element {
  const received = getCurrencyDifference(
    period.actualIncome.tracked,
    period.unplannedIncome.tracked,
  );
  const expected = period.expectedIncome.tracked;
  const share =
    expected > 0 ? Math.min(Math.max(received / expected, 0), 1) : 0;
  return (
    <Paper
      component={Link}
      href={routes.workspaceDetail(period.id, {})}
      sx={{
        display: "flex",
        flexDirection: "column",
        gap: 1.5,
        p: 2,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        color: "inherit",
        textDecoration: "none",
        "&:hover": { borderColor: "primary.main" },
      }}
    >
      <Stack
        direction="row"
        spacing={1}
        sx={{ alignItems: "center", justifyContent: "space-between" }}
      >
        <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
          {period.name}
        </Typography>
        {period.isOpen ? (
          <Chip label="Open" color="success" size="small" variant="outlined" />
        ) : null}
      </Stack>
      <Stack spacing={0.25}>
        <Typography variant="body2" color="text.secondary">
          Net worth
        </Typography>
        <Typography variant="h5" component="p" sx={{ fontWeight: 500 }}>
          {formatCurrency(period.closingBalance)}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          <SignedChange
            amount={getCurrencyDifference(
              period.closingBalance,
              period.openingBalance,
            )}
          />{" "}
          this period
        </Typography>
      </Stack>
      <Stack spacing={0.75}>
        <Stack
          direction="row"
          spacing={1}
          sx={{ justifyContent: "space-between" }}
        >
          <Typography variant="body2" color="text.secondary">
            Tracked income
          </Typography>
          <Typography variant="body2">
            {formatCurrency(received)} of {formatCurrency(expected)}
          </Typography>
        </Stack>
        <Box
          sx={{
            height: 8,
            borderRadius: 4,
            overflow: "hidden",
            bgcolor: "action.hover",
          }}
        >
          <Box
            sx={{
              width: `${share * 100}%`,
              height: "100%",
              bgcolor: "primary.main",
            }}
          />
        </Box>
      </Stack>
    </Paper>
  );
};

export default AccountingPeriodCard;
