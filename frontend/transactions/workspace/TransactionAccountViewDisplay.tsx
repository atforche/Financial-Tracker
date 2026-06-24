import { Stack, TextField } from "@mui/material";
import type { JSX } from "react";
import type { TransactionAccount } from "@/transactions/transaction";
import TransactionBalanceDetails from "./TransactionBalanceDetails";

interface TransactionAccountViewDisplayProps {
  readonly account: TransactionAccount;
}

/**
 * Displays a transaction account in a workspace view.
 */
const TransactionAccountViewDisplay = function ({
  account,
}: TransactionAccountViewDisplayProps): JSX.Element {
  return (
    <Stack spacing={0.75}>
      <TextField
        label="Account"
        value={account.accountName}
        variant="outlined"
        slotProps={{
          input: {
            readOnly: true,
          },
        }}
      />
      <TransactionBalanceDetails
        previousPostedBalance={account.previousAccountBalance.postedBalance}
        newPostedBalance={account.newAccountBalance.postedBalance}
      />
    </Stack>
  );
};

export default TransactionAccountViewDisplay;
