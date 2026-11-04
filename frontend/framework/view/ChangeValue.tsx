import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import { Box } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the ChangeValue component.
 */
interface ChangeValueProps {
  readonly startingValue: number;
  readonly endingValue?: number;
  readonly isIncreaseGood?: boolean;
}

/**
 * Displays a currency change and its percentage colored by whether it is good. By default an increase is good.
 */
const ChangeValue = function ({
  startingValue,
  endingValue,
  isIncreaseGood = true,
}: ChangeValueProps): JSX.Element {
  const netChange = getCurrencyDifference(
    endingValue ?? startingValue,
    startingValue,
  );
  const percentChange =
    startingValue === 0 ? 0 : (netChange / Math.abs(startingValue)) * 100;

  return (
    <Box
      component="span"
      sx={{
        color:
          netChange === 0 || netChange > 0 === isIncreaseGood
            ? "success.main"
            : "error.main",
      }}
    >
      {formatCurrency(netChange)} ({netChange >= 0 ? "+" : ""}
      {percentChange.toFixed(2)}%)
    </Box>
  );
};

export default ChangeValue;
