import type { JSX } from "react";
import type { SpendingTransaction } from "@/transactions/spendingTransaction";
import type { TransactionAccountDraft } from "@/transactions/transaction";
import TransactionAccountViewFrame from "@/transactions/workspace/TransactionAccountViewFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the SpendingTransactionSourceViewFrame component.
 */
interface SpendingTransactionSourceViewFrameProps {
  readonly transaction: SpendingTransaction;
  readonly account: TransactionAccountDraft | null;
}

/**
 * Displays the read-only source frame for a spending transaction.
 */
const SpendingTransactionSourceViewFrame = function ({
  transaction,
  account,
}: SpendingTransactionSourceViewFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title="Source">
      <TransactionAccountViewFrame
        transaction={transaction}
        account={account}
        label="Source Account"
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default SpendingTransactionSourceViewFrame;
