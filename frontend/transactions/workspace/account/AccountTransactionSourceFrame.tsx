import type {
  Account,
  AccountBalanceEvent,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import { Box, Stack } from "@mui/material";
import type { Location, LocationDraft } from "@/locations/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/transactions/workspace/TransactionSection";
import type { JSX } from "react";
import type { Transaction } from "@/transactions/types";
import TransactionAccountOrLocationFrame from "@/transactions/workspace/TransactionAccountOrLocationFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the AccountTransactionSourceFrame component.
 */
interface AccountTransactionSourceFrameProps {
  readonly accounts: AccountWithBalance[];
  readonly transaction?: Transaction | null;
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly locations?: readonly Location[] | undefined;
  readonly location: LocationDraft | null;
  readonly setLocation: ((location: LocationDraft | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly accountFilter?: ((account: Account) => boolean) | null;
  readonly color?: FrameColor;
  readonly accountBalanceEvent?: AccountBalanceEvent | null;
  readonly readOnly?: boolean;
}

/**
 * Displays a form frame for an account transaction source.
 */
const AccountTransactionSourceFrame = function ({
  accounts,
  transaction = null,
  account,
  setAccount,
  location,
  locations,
  setLocation,
  accountFilter = null,
  amount,
  setAmount,
  color = "info",
  accountBalanceEvent = null,
  readOnly = false,
}: AccountTransactionSourceFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title="Source" color={color}>
      <Stack
        // Viewing keeps the name and amount on one row on phones, since neither needs an input's width
        direction={{ xs: readOnly ? "row" : "column", sm: "row" }}
        spacing={2}
        alignItems={{
          xs: readOnly ? "flex-start" : "stretch",
          sm: "flex-start",
        }}
      >
        <Box sx={{ flex: "1 1 auto", minWidth: 0 }}>
          <TransactionAccountOrLocationFrame
            accounts={accounts}
            transaction={transaction}
            account={account}
            setAccount={readOnly ? null : setAccount}
            accountCaption="Source Account"
            locationCaption="Source Location"
            entryCaption="Source"
            locations={locations}
            location={location}
            setLocation={readOnly ? null : setLocation}
            accountFilter={accountFilter}
            balanceEvent={accountBalanceEvent}
            readOnly={readOnly}
          />
        </Box>
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

export default AccountTransactionSourceFrame;
