import type { Account, AccountIdentifier } from "@/accounts/types";
import AccountOrLocationEntryFrame from "@/transactions/workspace/AccountOrLocationEntryFrame";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the AccountTransactionSourceFormFrame component.
 */
interface AccountTransactionSourceFormFrameProps {
  readonly accounts: Account[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly location: string;
  readonly setLocation: ((location: string) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly accountFilter?: ((account: AccountIdentifier) => boolean) | null;
}

/**
 * Displays a form frame for an account transaction source.
 */
const AccountTransactionSourceFormFrame = function ({
  accounts,
  account,
  setAccount,
  location,
  setLocation,
  accountFilter = null,
  amount,
  setAmount,
}: AccountTransactionSourceFormFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title="Transfer Source">
      <AccountOrLocationEntryFrame
        accountCaption="Source Account"
        accounts={accounts}
        account={account}
        setAccount={setAccount}
        locationCaption="Source Location"
        location={location}
        setLocation={setLocation}
        accountFilter={accountFilter}
      />
      <CurrencyEntryField label="Amount" value={amount} setValue={setAmount} />
    </TransactionSourceOrDestinationFrame>
  );
};

export default AccountTransactionSourceFormFrame;
