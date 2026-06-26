import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";
import type { TransactionFund } from "@/transactions/transaction";
import TransactionFundViewDisplay from "@/transactions/workspace/TransactionFundViewDisplay";

/**
 * Props for the FundTransactionDestinationViewFrame component.
 */
interface FundTransactionDestinationViewFrameProps {
  readonly index: number;
  readonly fund: TransactionFund;
  readonly amount: number | null;
}

/**
 * Displays the read-only destination frame for one fund transaction destination.
 */
const FundTransactionDestinationViewFrame = function ({
  index,
  fund,
  amount,
}: FundTransactionDestinationViewFrameProps): JSX.Element {
  return (
    <TransactionFrame title={`Destination ${index + 1}`}>
      <TransactionFundViewDisplay fund={fund} />
      <CurrencyEntryField label="Amount" value={amount} />
    </TransactionFrame>
  );
};

export default FundTransactionDestinationViewFrame;
