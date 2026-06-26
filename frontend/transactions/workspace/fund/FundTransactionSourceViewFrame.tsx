import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";
import type { TransactionFund } from "@/transactions/transaction";
import TransactionFundViewDisplay from "@/transactions/workspace/TransactionFundViewDisplay";

/**
 * Props for the FundTransactionSourceViewFrame component.
 */
interface FundTransactionSourceViewFrameProps {
  readonly fund: TransactionFund;
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
    <TransactionFrame title="Source">
      <TransactionFundViewDisplay fund={fund} />
      <CurrencyEntryField label="Amount" value={amount} />
    </TransactionFrame>
  );
};

export default FundTransactionSourceViewFrame;
