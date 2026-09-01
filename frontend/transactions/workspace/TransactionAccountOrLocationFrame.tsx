import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import { Box, Stack, Typography } from "@mui/material";
import type { Location, LocationDraft } from "@/locations/types";

import AccountBalanceEventFrame from "@/transactions/workspace/AccountBalanceEventFrame";
import InsetFrame from "@/framework/view/InsetFrame";
import type { JSX } from "react";
import LocationEntryField from "@/locations/LocationEntryField";
import StringEntryField from "@/framework/forms/StringEntryField";
import type { Transaction } from "@/transactions/types";

/**
 * Props for the TransactionAccountOrLocationFrame component.
 */
interface TransactionAccountOrLocationFrameProps {
  readonly accounts: AccountWithBalance[];
  readonly transaction?: Transaction | null;
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly accountCaption?: string;
  readonly locationCaption: string;
  readonly locations?: readonly Location[] | undefined;
  readonly location: LocationDraft | null;
  readonly setLocation: ((location: LocationDraft | null) => void) | null;
  readonly accountFilter?: ((account: Account) => boolean) | null;
  readonly balanceChange?: number | null;
  readonly readOnly?: boolean;
}

/**
 * Displays a shared framed account-or-location entry block for transaction forms.
 */
const TransactionAccountOrLocationFrame = function ({
  accounts,
  transaction = null,
  account,
  setAccount,
  accountCaption = "Account",
  locationCaption,
  locations,
  location,
  setLocation,
  accountFilter = null,
  balanceChange = null,
  readOnly = false,
}: TransactionAccountOrLocationFrameProps): JSX.Element {
  const hasAccount = account !== null;
  const hasLocation = (location?.name ?? "").trim() !== "";

  if (readOnly && hasAccount && !hasLocation) {
    return (
      <AccountBalanceEventFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={null}
        accountFilter={accountFilter}
        label={accountCaption}
        balanceChange={balanceChange}
        inset
      />
    );
  }

  if (readOnly && !hasAccount) {
    return (
      <StringEntryField
        label={locationCaption}
        value={location?.name ?? null}
        setValue={null}
      />
    );
  }

  const accountOrLocationContent = (
    <Stack
      direction={{ xs: "column", md: "row" }}
      spacing={2}
      alignItems={{ xs: "stretch", md: "flex-start" }}
    >
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <AccountBalanceEventFrame
          accounts={accounts}
          transaction={transaction}
          account={account}
          setAccount={readOnly ? null : setAccount}
          accountFilter={accountFilter}
          label={accountCaption}
          balanceChange={balanceChange}
          inset={readOnly}
        />
      </Box>
      <Typography
        align="center"
        variant="subtitle1"
        sx={{ flexShrink: 0, minWidth: { md: 40 }, pt: { md: 2 } }}
      >
        Or
      </Typography>
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <LocationEntryField
          label={locationCaption}
          locations={locations}
          value={location}
          setValue={readOnly ? null : setLocation}
          disabled={!readOnly && account !== null}
        />
      </Box>
    </Stack>
  );

  return readOnly ? (
    accountOrLocationContent
  ) : (
    <InsetFrame>{accountOrLocationContent}</InsetFrame>
  );
};

export default TransactionAccountOrLocationFrame;
