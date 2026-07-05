import type { Account, AccountIdentifier } from "@/accounts/types";
import type {
  Transaction,
  TransactionAccountDraft,
} from "@/transactions/transaction";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import TransactionAccountViewFrame from "@/transactions/workspace/TransactionAccountViewFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the SpendingTransactionSourceFormFrame component.
 */
interface SpendingTransactionSourceFormFrameProps {
  readonly accounts: Account[];
  readonly transaction?: Transaction | null;
  readonly account: TransactionAccountDraft | null;
  readonly setAccount:
    ((account: TransactionAccountDraft | null) => void) | null;
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
  transaction = null,
  account,
  setAccount,
  amount,
  setAmount,
  accountFilter = null,
  color = "info",
}: SpendingTransactionSourceFormFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title="Source" color={color}>
      <TransactionAccountViewFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={setAccount}
        accountFilter={accountFilter}
        label="Source Account"
        balanceChange={amount === null ? null : -amount}
      />
      <CurrencyEntryField label="Amount" value={amount} setValue={setAmount} />
    </TransactionSourceOrDestinationFrame>
  );
};

export default SpendingTransactionSourceFormFrame;
