import type { JSX, ReactNode } from "react";
import { Stack } from "@mui/material";
import type { TransactionAccount } from "@/transactions/transaction";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

/**
 * Props for the SpendingTransactionSourceViewFrame component.
 */
interface SpendingTransactionSourceViewFrameProps {
  readonly account: TransactionAccount;
  readonly helperContent?: ReactNode;
}

/**
 * Displays the read-only source frame for a spending transaction.
 */
const SpendingTransactionSourceViewFrame = function ({
  account,
  helperContent = null,
}: SpendingTransactionSourceViewFrameProps): JSX.Element {
  return (
    <TransactionFrame title="Source" description="">
      <TransactionDisplayField
        label="Source Account"
        value={account.accountName}
        helperText={
          <Stack spacing={1.25}>
            <TransactionBalanceDetails
              previousPostedBalance={
                account.previousAccountBalance.postedBalance
              }
              newPostedBalance={account.newAccountBalance.postedBalance}
            />
            {helperContent}
          </Stack>
        }
      />
    </TransactionFrame>
  );
};

export default SpendingTransactionSourceViewFrame;
