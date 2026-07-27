import { Stack, Typography } from "@mui/material";
import type { FundGoalProgress } from "@/fund-goals/types";
import type { JSX } from "react";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the FundGoalAvailableBalance component.
 */
interface FundGoalAvailableBalanceProps {
  readonly availableBalance: FundGoalProgress["availableBalance"];
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
      <Typography
        variant="body2"
        fontWeight={700}
        color={
          availableBalance.currentBalance >= 0 ? "success.main" : "error.main"
        }
      >
        {formatCurrency(availableBalance.currentBalance)}
      </Typography>
    </Stack>
  );
};

export default FundGoalAvailableBalance;
