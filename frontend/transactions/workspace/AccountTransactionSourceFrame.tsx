import type { Account, AccountIdentifier } from "@/accounts/types";
import AccountEntryField from "@/accounts/AccountEntryField";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

interface AccountTransactionSourceFrameProps {
  readonly accounts: Account[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly location: string;
  readonly setLocation: ((location: string) => void) | null;
  readonly filter?: ((account: AccountIdentifier) => boolean) | null;
}

/**
 * Displays the source frame for an account transaction.
 */
const AccountTransactionSourceFrame = function ({
  accounts,
  account,
  setAccount,
  location,
  setLocation,
  filter = null,
}: AccountTransactionSourceFrameProps): JSX.Element {
  return (
    <TransactionFrame
      title="Transfer Source"
      description="Choose the source account or provide the source location for this transfer."
    >
      <AccountEntryField
        label="Source Account"
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
        label="Source Location"
        value={location}
        setValue={account === null ? setLocation : null}
      />
    </TransactionFrame>
  );
};

export default AccountTransactionSourceFrame;
