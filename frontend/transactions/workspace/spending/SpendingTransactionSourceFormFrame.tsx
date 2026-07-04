import type { Account, AccountIdentifier } from "@/accounts/types";
import AccountEntryField from "@/accounts/AccountEntryField";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the SpendingTransactionSourceFormFrame component.
 */
interface SpendingTransactionSourceFormFrameProps {
  readonly accounts: Account[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly accountFilter?: ((account: AccountIdentifier) => boolean) | null;
  readonly color?: FrameColor;
}

/**
 * Displays the source frame for a spending transaction.
 */
const SpendingTransactionSourceFormFrame = function ({
  accounts,
  account,
  setAccount,
  amount,
  setAmount,
  accountFilter = null,
  color = "info",
}: SpendingTransactionSourceFormFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title="Source" color={color}>
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
        filter={accountFilter}
      />
      <CurrencyEntryField label="Amount" value={amount} setValue={setAmount} />
    </TransactionSourceOrDestinationFrame>
  );
};

export default SpendingTransactionSourceFormFrame;
