import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import AccountBalanceEventFrame from "@/transactions/workspace/AccountBalanceEventFrame";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import type { Transaction } from "@/transactions/types";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the SpendingTransactionSourceFrame component.
 */
interface SpendingTransactionSourceFrameProps {
  readonly accounts: AccountWithBalance[];
  readonly transaction?: Transaction | null;
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly accountFilter?: ((account: Account) => boolean) | null;
  readonly title?: string;
  readonly accountLabel?: string;
  readonly balanceChange?: number | null;
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
  title = "Source",
  accountLabel = "Source Account",
  balanceChange,
  color = "info",
  readOnly = false,
}: SpendingTransactionSourceFrameProps): JSX.Element {
  const effectiveBalanceChange =
    balanceChange === undefined
      ? amount === null
        ? null
        : -amount
      : balanceChange;

  return (
    <TransactionSourceOrDestinationFrame title={title} color={color}>
      <Stack
        direction={{ xs: "column", sm: "row" }}
        spacing={2}
        alignItems={{ xs: "stretch", sm: "flex-start" }}
      >
        <AccountBalanceEventFrame
          accounts={accounts}
          transaction={transaction}
          account={account}
          setAccount={readOnly ? null : setAccount}
          accountFilter={accountFilter}
          label={accountLabel}
          balanceChange={effectiveBalanceChange}
          sx={{ flex: { sm: "1 1 auto" }, minWidth: 0 }}
        />
        <CurrencyEntryField
          label="Amount"
          value={amount}
          setValue={readOnly ? null : setAmount}
          sx={{ width: { xs: "100%", sm: 220 } }}
        />
      </Stack>
    </TransactionSourceOrDestinationFrame>
  );
};

export default SpendingTransactionSourceFrame;
