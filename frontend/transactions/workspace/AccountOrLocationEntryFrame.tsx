import type { Account, AccountIdentifier } from "@/accounts/types";
import { Box, Stack, Typography } from "@mui/material";
import AccountEntryField from "@/accounts/AccountEntryField";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the AccountOrLocationEntryFrame component.
 */
interface AccountOrLocationEntryFrameProps {
  readonly accountCaption: string;
  readonly accounts: Account[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly locationCaption: string;
  readonly location: string | null;
  readonly setLocation: ((location: string) => void) | null;
  readonly accountFilter?: ((account: AccountIdentifier) => boolean) | null;
}

/**
 * Displays a shared framed account-or-location entry block for transaction forms.
 */
const AccountOrLocationEntryFrame = function ({
  accountCaption,
  accounts,
  account,
  setAccount,
  locationCaption,
  location,
  setLocation,
  accountFilter = null,
}: AccountOrLocationEntryFrameProps): JSX.Element {
  return (
    <Stack
      direction={{ xs: "column", md: "row" }}
      spacing={2}
      alignItems={{ xs: "stretch", md: "center" }}
    >
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <AccountEntryField
          label={accountCaption}
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
                  setLocation?.("");
                }
          }
          filter={accountFilter}
        />
      </Box>
      <Typography
        align="center"
        variant="subtitle1"
        sx={{ flexShrink: 0, minWidth: { md: 40 } }}
      >
        Or
      </Typography>
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <StringEntryField
          label={locationCaption}
          value={location}
          setValue={account === null ? setLocation : null}
        />
      </Box>
    </Stack>
  );
};

export default AccountOrLocationEntryFrame;
