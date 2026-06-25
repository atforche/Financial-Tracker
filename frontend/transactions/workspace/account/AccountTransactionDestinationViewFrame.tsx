import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import type { TransactionAccount } from "@/transactions/transaction";
import TransactionAccountViewDisplay from "@/transactions/workspace/TransactionAccountViewDisplay";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

/**
 * Props for the AccountTransactionDestinationViewFrame component.
 */
interface AccountTransactionDestinationViewFrameProps {
  readonly index: number;
  readonly account: TransactionAccount | null;
  readonly location: string;
  readonly amount: number | null;
}

/**
 * Displays a view frame for an account transaction destination.
 */
const AccountTransactionDestinationViewFrame = function ({
  index,
  account,
  location,
  amount,
}: AccountTransactionDestinationViewFrameProps): JSX.Element {
  return (
    <TransactionFrame title={`Destination ${index + 1}`}>
      {account !== null && <TransactionAccountViewDisplay account={account} />}
      {location !== "" && (
        <StringEntryField label="Location" value={location} />
      )}
      <CurrencyEntryField label="Amount" value={amount} />
    </TransactionFrame>
  );
};

export default AccountTransactionDestinationViewFrame;
