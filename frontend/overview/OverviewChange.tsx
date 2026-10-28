import {
  compareCurrencyAmounts,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import { Box } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the OverviewChange component.
 */
interface OverviewChangeProps {
  readonly amount: number;
  readonly isIncreaseGood?: boolean;
}

/**
 * Gets the text color for a change: green when it is good, red when it is bad, and grey when there is none.
 */
const getChangeColor = function (
  change: number,
  isIncreaseGood: boolean,
): string {
  const comparison = compareCurrencyAmounts(change, 0);
  if (comparison === 0) {
    return "text.secondary";
  }
  return comparison > 0 === isIncreaseGood ? "success.main" : "error.main";
};

/**
 * Displays a signed change amount in green when it is good and red when it is bad. By default an increase is good.
 */
const OverviewChange = function ({
  amount,
  isIncreaseGood = true,
}: OverviewChangeProps): JSX.Element {
  return (
    <Box
      component="span"
      sx={{ color: getChangeColor(amount, isIncreaseGood), fontWeight: 500 }}
    >
      {formatSignedCurrency(amount)}
    </Box>
  );
};

export default OverviewChange;
