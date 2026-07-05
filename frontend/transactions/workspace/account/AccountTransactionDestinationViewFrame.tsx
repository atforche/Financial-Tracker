import type { AccountTransaction } from "@/transactions/accountTransaction";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import type { TransactionAccountDraft } from "@/transactions/transaction";
import TransactionAccountFrame from "@/transactions/workspace/TransactionAccountFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the AccountTransactionDestinationViewFrame component.
 */
interface AccountTransactionDestinationViewFrameProps {
  readonly transaction: AccountTransaction;
  readonly index: number;
  readonly account: TransactionAccountDraft | null;
  readonly location: string;
  readonly amount: number | null;
}

/**
 * Displays a view frame for an account transaction destination.
 */
const AccountTransactionDestinationViewFrame = function ({
  transaction,
  index,
  account,
  location,
  amount,
}: AccountTransactionDestinationViewFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title={`Destination ${index + 1}`}>
      {account === null ? null : (
        <TransactionAccountFrame transaction={transaction} account={account} />
      )}
      {location !== "" && (
        <StringEntryField label="Location" value={location} />
      )}
      <CurrencyEntryField label="Amount" value={amount} />
    </TransactionSourceOrDestinationFrame>
  );
};

export default AccountTransactionDestinationViewFrame;
