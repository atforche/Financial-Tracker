import type { Account, AccountIdentifier } from "@/accounts/types";
import AccountOrLocationEntryFrame from "@/transactions/workspace/AccountOrLocationEntryFrame";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

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
  readonly onRemove?: (() => void) | null;
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
  onRemove = null,
}: AccountTransactionDestinationFrameProps): JSX.Element {
  return (
    <TransactionFrame
      title={`Destination ${index + 1}`}
      description="Capture where this portion of the transfer is going."
      onRemove={onRemove}
    >
      <AccountOrLocationEntryFrame
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
    </TransactionFrame>
  );
};

export default AccountTransactionDestinationFormFrame;
