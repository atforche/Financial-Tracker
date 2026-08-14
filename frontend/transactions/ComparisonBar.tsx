"use client";

import { Box, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the ComparisonBar component.
 */
interface ComparisonBarProps {
  readonly label: string;
  readonly amount: number;
  readonly amountColor: string;
  readonly difference: number;
  readonly differenceLabel: string;
  readonly differenceColor: string;
  readonly maxAmount: number;
}

/**
 * Displays an amount and an optional difference as a proportional bar.
 */
const ComparisonBar = function ({
  label,
  amount,
  amountColor,
  difference,
  differenceLabel,
  differenceColor,
  maxAmount,
}: ComparisonBarProps): JSX.Element {
  const amountRatio = amount / maxAmount;
  const differenceRatio = difference / maxAmount;
  const differenceCaption =
    difference > 0
      ? `${differenceLabel}: ${formatCurrency(difference)}`
      : undefined;

  return (
    <Stack spacing={0.75}>
      <Stack direction="row" sx={{ width: "100%" }} justifyContent="space-between">
        <Typography variant="body2" fontWeight={600} color={amountColor} noWrap>
          {label}: {formatCurrency(amount)}
        </Typography>
        {differenceCaption !== undefined && (
          <Typography
            variant="body2"
            fontWeight={600}
            color={differenceColor}
            noWrap
          >
            {differenceCaption}
          </Typography>
        )}
      </Stack>
      <Box
        role="img"
        aria-label={`${label} ${formatCurrency(amount)}${differenceCaption === undefined ? "" : `, ${differenceCaption}`}`}
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
            width: `${Math.round(amountRatio * 100)}%`,
            height: "100%",
            backgroundColor: amountColor,
          }}
        />
        {difference > 0 && (
          <Box
            sx={{
              width: `${Math.round(differenceRatio * 100)}%`,
              height: "100%",
              backgroundColor: differenceColor,
            }}
          />
        )}
      </Box>
    </Stack>
  );
};

export default ComparisonBar;
