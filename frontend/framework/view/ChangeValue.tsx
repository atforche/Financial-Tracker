import { Box } from "@mui/material";
import type { JSX } from "react";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the ChangeValue component.
 */
interface ChangeValueProps {
  readonly startingValue: number;
  readonly endingValue?: number;
  readonly change?: number;
}

/**
 * Displays a currency change and its percentage with signed coloring.
 */
const ChangeValue = function ({
  startingValue,
  endingValue,
  change,
}: ChangeValueProps): JSX.Element {
  const netChange = change ?? (endingValue ?? startingValue) - startingValue;
  const percentChange =
    startingValue === 0 ? 0 : (netChange / Math.abs(startingValue)) * 100;

  return (
    <Box
      component="span"
      sx={{ color: netChange >= 0 ? "success.main" : "error.main" }}
    >
      {formatCurrency(netChange)} ({netChange >= 0 ? "+" : ""}
      {percentChange.toFixed(2)}%)
    </Box>
  );
};

export default ChangeValue;
