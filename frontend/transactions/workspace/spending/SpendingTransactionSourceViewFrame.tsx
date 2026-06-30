import type { JSX } from "react";
import type { SpendingTransaction } from "@/transactions/spendingTransaction";
import type { TransactionAccount } from "@/transactions/transaction";
import TransactionAccountViewFrame from "@/transactions/workspace/TransactionAccountViewFrame";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

/**
 * Props for the SpendingTransactionSourceViewFrame component.
 */
interface SpendingTransactionSourceViewFrameProps {
  readonly transaction: SpendingTransaction;
  readonly account: TransactionAccount;
}

/**
 * Displays the read-only source frame for a spending transaction.
 */
const SpendingTransactionSourceViewFrame = function ({
  transaction,
  account,
}: SpendingTransactionSourceViewFrameProps): JSX.Element {
  return (
    <TransactionFrame title="Source" description="">
      <TransactionAccountViewFrame
        transaction={transaction}
        account={account}
        label="Source Account"
      />
    </TransactionFrame>
  );
};

export default SpendingTransactionSourceViewFrame;
