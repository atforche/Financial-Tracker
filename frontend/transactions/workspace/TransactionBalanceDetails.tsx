import { Chip, Stack } from "@mui/material";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the TransactionBalanceDetails component.
 */
interface TransactionBalanceDetailsProps {
  readonly previousPostedBalance: number;
  readonly newPostedBalance: number;
  readonly newBalanceLabel?: string;
}

/**
 * Displays compact previous and new posted balances for a transaction entity.
 */
const TransactionBalanceDetails = function ({
  previousPostedBalance,
  newPostedBalance,
  newBalanceLabel = "New",
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
        label={`${newBalanceLabel} ${formatCurrency(newPostedBalance)}`}
        size="small"
        color="primary"
        variant="outlined"
      />
    </Stack>
  );
};

export default TransactionBalanceDetails;
