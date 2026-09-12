import { Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the FundGoalEndingBalance component.
 */
interface FundGoalEndingBalanceProps {
  readonly endingBalance: number;
}

/**
 * Displays the ending balance for a Fund Goal.
 */
const FundGoalEndingBalance = function ({
  endingBalance,
}: FundGoalEndingBalanceProps): JSX.Element {
  return (
    <Stack direction="row" justifyContent="space-between" gap={2}>
      <Typography variant="body2" fontWeight={700}>
        Ending Balance
      </Typography>
      <Typography variant="body2" fontWeight={700}>
        {formatCurrency(endingBalance)}
      </Typography>
    </Stack>
  );
};

export default FundGoalEndingBalance;
