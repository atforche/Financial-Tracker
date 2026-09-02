import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import AccountBalanceEventFrame from "@/transactions/workspace/AccountBalanceEventFrame";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import type { Transaction } from "@/transactions/types";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the RefundTransactionDestinationFrame component.
 */
interface RefundTransactionDestinationFrameProps {
  readonly accounts: AccountWithBalance[];
  readonly transaction?: Transaction | null;
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly amount: number | null;
  readonly filter?: ((account: Account) => boolean) | null;
  readonly color?: FrameColor;
  readonly readOnly?: boolean;
}

/**
 * Displays the destination frame for a refund transaction.
 */
const RefundTransactionDestinationFrame = function ({
  accounts,
  transaction = null,
  account,
  setAccount,
  amount,
  filter = null,
  color = "info",
  readOnly = false,
}: RefundTransactionDestinationFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title="Destination" color={color}>
      <AccountBalanceEventFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={readOnly ? null : setAccount}
        label="Destination Account"
        accountFilter={filter}
        balanceChange={amount}
        inset
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default RefundTransactionDestinationFrame;
