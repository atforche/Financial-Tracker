"use client";

import { Stack, Typography } from "@mui/material";
import type { Account } from "@/accounts/types";
import AccountCurrentBalanceFrame from "@/accounts/workspace/AccountCurrentBalanceFrame";
import AccountDetailsFrame from "@/accounts/workspace/AccountDetailsFrame";
import type { JSX } from "react";

/**
 * Props for the ViewAccountForm component.
 */
interface ViewAccountFormProps {
  readonly account: Account;
}

/**
 * Displays the read-only account workspace view for a selected account.
 */
const ViewAccountForm = function ({
  account,
}: ViewAccountFormProps): JSX.Element {
  return (
    <Stack spacing={3} sx={{ width: "100%", maxWidth: 1200 }}>
      <Typography variant="h5">View Account</Typography>
      <AccountDetailsFrame
        color="info"
        name={account.name}
        setName={null}
        nameErrorMessage={null}
        accountType={account.type}
        setAccountType={null}
        accountTypeErrorMessage={null}
      />
      <AccountCurrentBalanceFrame account={account} />
    </Stack>
  );
};

export default ViewAccountForm;
