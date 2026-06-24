import { Chip, Stack } from "@mui/material";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface TransactionBalanceDetailsProps {
  readonly previousPostedBalance: number;
  readonly newPostedBalance: number;
}

/**
 * Displays compact previous and new posted balances for a transaction entity.
 */
const TransactionBalanceDetails = function ({
  previousPostedBalance,
  newPostedBalance,
}: TransactionBalanceDetailsProps): JSX.Element {
  return (
    <Stack
      direction="row"
      spacing={1}
      flexWrap="wrap"
      useFlexGap
      sx={{ px: 1.75 }}
    >
      <Chip
        label={`Previous ${formatCurrency(previousPostedBalance)}`}
        size="small"
        variant="outlined"
      />
      <Chip
        label={`New ${formatCurrency(newPostedBalance)}`}
        size="small"
        color="primary"
        variant="outlined"
      />
    </Stack>
  );
};

export default TransactionBalanceDetails;
