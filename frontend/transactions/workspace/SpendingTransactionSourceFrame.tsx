import type { Account, AccountIdentifier } from "@/accounts/types";
import AccountEntryField from "@/accounts/AccountEntryField";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";
import type { JSX } from "react";

interface SpendingTransactionSourceFrameProps {
  readonly accounts: Account[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly filter?: ((account: AccountIdentifier) => boolean) | null;
}

/**
 * Displays the source frame for a spending transaction.
 */
const SpendingTransactionSourceFrame = function ({
  accounts,
  account,
  setAccount,
  filter = null,
}: SpendingTransactionSourceFrameProps): JSX.Element {
  return (
    <TransactionFrame
      title="SpendingTransactionSource"
      description="Choose the tracked account that the spending will come from."
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
                  accounts.find((candidate) => candidate.id === nextValue?.id) ??
                    null,
                );
              }
        }
        filter={filter}
      />
    </TransactionFrame>
  );
};

export default SpendingTransactionSourceFrame;
