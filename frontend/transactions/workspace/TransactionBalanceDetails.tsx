import { Chip, Stack } from "@mui/material";
import type { JSX } from "react";

interface TransactionBalanceDetailsProps {
  readonly previousPostedBalance: string;
  readonly newPostedBalance: string;
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
        label={`Previous ${previousPostedBalance}`}
        size="small"
        variant="outlined"
      />
      <Chip
        label={`New ${newPostedBalance}`}
        size="small"
        color="primary"
        variant="outlined"
      />
    </Stack>
  );
};

export default TransactionBalanceDetails;
