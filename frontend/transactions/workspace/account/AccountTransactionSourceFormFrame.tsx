import type { Account, AccountIdentifier } from "@/accounts/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import TransactionAccountOrLocationFrame from "@/transactions/workspace/TransactionAccountOrLocationFrame";
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
  readonly color?: FrameColor;
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
  color = "info",
}: AccountTransactionSourceFormFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title="Source" color={color}>
      <TransactionAccountOrLocationFrame
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
