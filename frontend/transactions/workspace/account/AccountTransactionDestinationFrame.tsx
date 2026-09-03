import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import { Box, Stack } from "@mui/material";
import type { Location, LocationDraft } from "@/locations/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import type { Transaction } from "@/transactions/types";
import TransactionAccountOrLocationFrame from "@/transactions/workspace/TransactionAccountOrLocationFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the AccountTransactionDestinationFrame component.
 */
interface AccountTransactionDestinationFrameProps {
  readonly index: number;
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
  readonly onRemove?: (() => void) | null;
  readonly color?: FrameColor;
  readonly readOnly?: boolean;
  readonly autoFocus?: boolean;
}

/**
 * Displays a form frame for an account transaction destination.
 */
const AccountTransactionDestinationFrame = function ({
  index,
  accounts,
  transaction = null,
  account,
  setAccount,
  location,
  locations,
  setLocation,
  amount,
  setAmount,
  accountFilter = null,
  onRemove = null,
  color = "info",
  readOnly = false,
  autoFocus = false,
}: AccountTransactionDestinationFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onRemove={readOnly ? null : onRemove}
      color={color}
    >
      <Stack
        direction={{ xs: "column", sm: "row" }}
        spacing={2}
        alignItems={{ xs: "stretch", sm: "flex-start" }}
      >
        <Box sx={{ flex: { sm: "1 1 auto" }, minWidth: 0 }}>
          <TransactionAccountOrLocationFrame
            accounts={accounts}
            transaction={transaction}
            account={account}
            setAccount={readOnly ? null : setAccount}
            accountCaption="Account"
            locationCaption="Location"
            entryCaption="Destination"
            locations={locations}
            location={location}
            setLocation={readOnly ? null : setLocation}
            accountFilter={accountFilter}
            balanceChange={amount}
            readOnly={readOnly}
            autoFocus={autoFocus}
          />
        </Box>
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

export default AccountTransactionDestinationFrame;
