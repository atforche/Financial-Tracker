import { Stack, TextField } from "@mui/material";
import type { JSX } from "react";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import type { TransactionFund } from "@/transactions/transaction";

/**
 * Props for the TransactionFundViewDisplay component.
 */
interface TransactionFundViewDisplayProps {
  readonly fund: TransactionFund;
}

/**
 * Displays a transaction fund in a workspace view.
 */
const TransactionFundViewDisplay = function ({
  fund,
}: TransactionFundViewDisplayProps): JSX.Element {
  return (
    <Stack spacing={0.75}>
      <TextField
        label="Fund"
        value={fund.fundName}
        variant="outlined"
        slotProps={{
          input: {
            readOnly: true,
          },
        }}
      />
      <TransactionBalanceDetails
        previousPostedBalance={fund.previousFundBalance.postedBalance}
        newPostedBalance={fund.newFundBalance.postedBalance}
      />
    </Stack>
  );
};

export default TransactionFundViewDisplay;
