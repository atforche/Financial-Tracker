import type { Account, AccountIdentifier } from "@/accounts/types";
import AccountEntryField from "@/accounts/AccountEntryField";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

interface AccountTransactionDestinationFrameProps {
  readonly index: number;
  readonly accounts: Account[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly location: string;
  readonly setLocation: ((location: string) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly filter?: ((account: AccountIdentifier) => boolean) | null;
  readonly onRemove?: (() => void) | null;
}

/**
 * Displays a destination frame for one account transaction destination.
 */
const AccountTransactionDestinationFrame = function ({
  index,
  accounts,
  account,
  setAccount,
  location,
  setLocation,
  amount,
  setAmount,
  filter = null,
  onRemove = null,
}: AccountTransactionDestinationFrameProps): JSX.Element {
  return (
    <TransactionFrame
      title={`Transfer Destination ${index + 1}`}
      description="Capture where this portion of the transfer is going."
      onRemove={onRemove}
    >
      <AccountEntryField
        label="Destination Account"
        options={accounts}
        value={account}
        setValue={
          setAccount === null
            ? null
            : (nextValue): void => {
                setAccount(
                  accounts.find(
                    (candidate) => candidate.id === nextValue?.id,
                  ) ?? null,
                );
              }
        }
        filter={filter}
      />
      <StringEntryField
        label="Destination Location"
        value={location}
        setValue={account === null ? setLocation : null}
      />
      <CurrencyEntryField
        label="Destination Amount"
        value={amount}
        setValue={setAmount}
      />
    </TransactionFrame>
  );
};

export default AccountTransactionDestinationFrame;
