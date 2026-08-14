"use client";

import { Box, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the IncomeBreakdownBar component.
 */
interface IncomeBreakdownBarProps {
  readonly total: number;
  readonly tracked: number;
  readonly untracked: number;
}

/**
 * Displays total income as tracked and untracked sections of a stacked bar.
 */
const IncomeBreakdownBar = function ({
  total,
  tracked,
  untracked,
}: IncomeBreakdownBarProps): JSX.Element {
  const totalForRatio = Math.max(total, 1);
  const trackedRatio = tracked / totalForRatio;
  const untrackedRatio = untracked / totalForRatio;

  return (
    <Stack spacing={0.75}>
      <Stack
        direction="row"
        sx={{ width: "100%" }}
        justifyContent="space-between"
      >
        <Typography
          variant="body2"
          color="success.main"
          fontWeight={600}
          noWrap
        >
          Tracked: {formatCurrency(tracked)}
        </Typography>
        <Typography
          variant="body2"
          color="text.secondary"
          textAlign="right"
          fontWeight={600}
          noWrap
          sx={{ width: "max-content" }}
        >
          Untracked: {formatCurrency(untracked)}
        </Typography>
      </Stack>
      <Box
        role="img"
        aria-label={`Total income ${formatCurrency(total)}, tracked ${formatCurrency(tracked)}, untracked ${formatCurrency(untracked)}`}
        sx={{
          display: "flex",
          width: "100%",
          height: 16,
          borderRadius: 1,
          backgroundColor: "divider",
          overflow: "hidden",
        }}
      >
        <Box
          sx={{
            width: `${Math.round(trackedRatio * 100)}%`,
            height: "100%",
            backgroundColor: "success.main",
          }}
        />
        <Box
          sx={{
            width: `${Math.round(untrackedRatio * 100)}%`,
            height: "100%",
            backgroundColor: "text.secondary",
          }}
        />
      </Box>
      <Typography
        variant="body2"
        color="text.secondary"
        textAlign="right"
        fontWeight={600}
        noWrap
      >
        Total: {formatCurrency(total)}
      </Typography>
    </Stack>
  );
};

export default IncomeBreakdownBar;
