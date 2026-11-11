import type {
  Account,
  AccountBalanceEvent,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import AccountBalanceEventFrame from "@/transactions/workspace/AccountBalanceEventFrame";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/transactions/workspace/TransactionSection";
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
  readonly color?: FrameColor;
  readonly accountBalanceEvent?: AccountBalanceEvent | null;
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
  color = "info",
  accountBalanceEvent = null,
  readOnly = false,
}: SpendingTransactionSourceFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title={title} color={color}>
      <Stack
        // Viewing keeps the name and amount on one row on phones, since neither needs an input's width
        direction={{ xs: readOnly ? "row" : "column", sm: "row" }}
        spacing={2}
        alignItems={{
          xs: readOnly ? "flex-start" : "stretch",
          sm: "flex-start",
        }}
      >
        <AccountBalanceEventFrame
          accounts={accounts}
          transaction={transaction}
          account={account}
          setAccount={readOnly ? null : setAccount}
          accountFilter={accountFilter}
          label={accountLabel}
          balanceEvent={accountBalanceEvent}
          sx={{ flex: "1 1 auto", minWidth: 0 }}
        />
        <CurrencyEntryField
          label="Amount"
          value={amount}
          setValue={readOnly ? null : setAmount}
          sx={{
            width: { xs: readOnly ? "auto" : "100%", sm: 220 },
            // Keeps a viewed amount whole beside a long name or balance chips
            flexShrink: 0,
            whiteSpace: "nowrap",
          }}
        />
      </Stack>
    </TransactionSourceOrDestinationFrame>
  );
};

export default SpendingTransactionSourceFrame;
