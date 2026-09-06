import { Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the FundGoalAvailableBalance component.
 */
interface FundGoalAvailableBalanceProps {
  readonly availableBalance: number;
}

/**
 * Displays the available balance for a Fund Goal.
 */
const FundGoalAvailableBalance = function ({
  availableBalance,
}: FundGoalAvailableBalanceProps): JSX.Element {
  return (
    <Stack direction="row" justifyContent="space-between" gap={2}>
      <Typography variant="body2" fontWeight={700}>
        Available Balance
      </Typography>
      <Typography variant="body2" fontWeight={700}>
        {formatCurrency(availableBalance)}
      </Typography>
    </Stack>
  );
};

export default FundGoalAvailableBalance;
