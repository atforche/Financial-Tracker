import { Box } from "@mui/material";
import type { JSX } from "react";
import { formatSignedCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the OverviewChange component.
 */
interface OverviewChangeProps {
  readonly amount: number;
}

/**
 * Displays a signed change amount in green when it rose and red when it fell.
 */
const OverviewChange = function ({ amount }: OverviewChangeProps): JSX.Element {
  return (
    <Box
      component="span"
      sx={{
        color:
          amount > 0
            ? "success.main"
            : amount < 0
              ? "error.main"
              : "text.secondary",
        fontWeight: 500,
      }}
    >
      {formatSignedCurrency(amount)}
    </Box>
  );
};

export default OverviewChange;
