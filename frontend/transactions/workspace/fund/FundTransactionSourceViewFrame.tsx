import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import type { TransactionFundDraft } from "@/transactions/transaction";
import TransactionFundFrame from "@/transactions/workspace/TransactionFundFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the FundTransactionSourceViewFrame component.
 */
interface FundTransactionSourceViewFrameProps {
  readonly fund: TransactionFundDraft | null;
  readonly amount: number | null;
}

/**
 * Displays the read-only source frame for a fund transaction.
 */
const FundTransactionSourceViewFrame = function ({
  fund,
  amount,
}: FundTransactionSourceViewFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title="Source">
      <TransactionFundFrame fund={fund} />
      <CurrencyEntryField label="Amount" value={amount} />
    </TransactionSourceOrDestinationFrame>
  );
};

export default FundTransactionSourceViewFrame;
