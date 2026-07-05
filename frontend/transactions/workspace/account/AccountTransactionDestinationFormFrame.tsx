import type { Account, AccountIdentifier } from "@/accounts/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import TransactionAccountOrLocationFrame from "@/transactions/workspace/TransactionAccountOrLocationFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the AccountTransactionDestinationFormFrame component.
 */
interface AccountTransactionDestinationFrameProps {
  readonly index: number;
  readonly accounts: Account[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
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
        accountCaption="Account"
        accounts={accounts}
        account={account}
        setAccount={setAccount}
        locationCaption="Location"
        location={location}
        setLocation={setLocation}
        accountFilter={accountFilter}
      />
      <CurrencyEntryField label="Amount" value={amount} setValue={setAmount} />
    </TransactionSourceOrDestinationFrame>
  );
};

export default AccountTransactionDestinationFormFrame;
