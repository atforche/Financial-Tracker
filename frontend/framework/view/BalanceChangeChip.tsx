import { Box, Chip, type ChipProps } from "@mui/material";
import { ArrowForward } from "@mui/icons-material";
import type { JSX } from "react";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the BalanceChangeChip component.
 */
interface BalanceChangeChipProps {
  readonly label: string;
  readonly previousValue: number;
  readonly newValue: number;
  readonly color?: ChipProps["color"];
  readonly size?: ChipProps["size"];
}

/**
 * Displays a compact currency change from a previous value to a new value.
 */
const BalanceChangeChip = function ({
  label,
  previousValue,
  newValue,
  color,
  size,
}: BalanceChangeChipProps): JSX.Element {
  return (
    <Chip
      variant="outlined"
      color={color}
      size={size}
      label={
        <Box
          component="span"
          sx={{
            display: "inline-flex",
            alignItems: "center",
            lineHeight: 1.2,
            gap: 0.5,
          }}
        >
          <span>
            {label}: {formatCurrency(previousValue)}
          </span>
          <ArrowForward aria-hidden="true" sx={{ fontSize: 16 }} />
          <span>{formatCurrency(newValue)}</span>
        </Box>
      }
    />
  );
};

export default BalanceChangeChip;
