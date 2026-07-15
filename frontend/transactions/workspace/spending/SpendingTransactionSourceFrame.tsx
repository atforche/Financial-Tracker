import type { AccountIdentifier, AccountWithBalance } from "@/accounts/types";
import type {
  Transaction,
  TransactionAccountDraft,
} from "@/transactions/transaction";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import TransactionAccountFrame from "@/transactions/workspace/TransactionAccountFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the SpendingTransactionSourceFrame component.
 */
interface SpendingTransactionSourceFrameProps {
  readonly accounts: AccountWithBalance[];
  readonly transaction?: Transaction | null;
  readonly account: TransactionAccountDraft | null;
  readonly setAccount:
    ((account: TransactionAccountDraft | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly accountFilter?: ((account: AccountIdentifier) => boolean) | null;
  readonly color?: FrameColor;
  readonly readOnly?: boolean;
}

/**
 * Displays the source frame for a spending transaction.
 */
const SpendingTransactionSourceFrame = function ({
  accounts,
  transaction = null,
  account,
  setAccount,
  amount,
  setAmount,
  accountFilter = null,
  color = "info",
  readOnly = false,
}: SpendingTransactionSourceFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title="Source" color={color}>
      <TransactionAccountFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={readOnly ? null : setAccount}
        accountFilter={accountFilter}
        label="Source Account"
        balanceChange={amount === null ? null : -amount}
      />
      <CurrencyEntryField
        label="Amount"
        value={amount}
        setValue={readOnly ? null : setAmount}
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default SpendingTransactionSourceFrame;
