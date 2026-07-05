import type { Account, AccountIdentifier } from "@/accounts/types";
import type {
  Transaction,
  TransactionAccountDraft,
} from "@/transactions/transaction";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import TransactionAccountOrLocationFrame from "@/transactions/workspace/TransactionAccountOrLocationFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the AccountTransactionDestinationFrame component.
 */
interface AccountTransactionDestinationFrameProps {
  readonly index: number;
  readonly accounts: Account[];
  readonly transaction?: Transaction | null;
  readonly account: TransactionAccountDraft | null;
  readonly setAccount:
    ((account: TransactionAccountDraft | null) => void) | null;
  readonly location: string;
  readonly setLocation: ((location: string) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly accountFilter?: ((account: AccountIdentifier) => boolean) | null;
  readonly onAdd?: (() => void) | null;
  readonly onRemove?: (() => void) | null;
  readonly color?: FrameColor;
}

/**
 * Displays a form frame for an account transaction destination.
 */
const AccountTransactionDestinationFormFrame = function ({
  index,
  accounts,
  transaction = null,
  account,
  setAccount,
  location,
  setLocation,
  amount,
  setAmount,
  accountFilter = null,
  onAdd = null,
  onRemove = null,
  color = "info",
}: AccountTransactionDestinationFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onAdd={onAdd}
      onRemove={onRemove}
      color={color}
    >
      <TransactionAccountOrLocationFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={setAccount}
        accountCaption="Account"
        locationCaption="Location"
        location={location}
        setLocation={setLocation}
        accountFilter={accountFilter}
        balanceChange={amount}
      />
      <CurrencyEntryField label="Amount" value={amount} setValue={setAmount} />
    </TransactionSourceOrDestinationFrame>
  );
};

export default AccountTransactionDestinationFormFrame;
