"use client";

import { Stack, Typography } from "@mui/material";
import type { Account } from "@/accounts/types";
import AccountCurrentBalanceFrame from "@/accounts/workspace/AccountCurrentBalanceFrame";
import AccountDetailsFrame from "@/accounts/workspace/AccountDetailsFrame";
import DeleteAccountForm from "@/accounts/workspace/DeleteAccountForm";
import type { JSX } from "react";
import UpdateAccountForm from "@/accounts/workspace/UpdateAccountForm";

/**
 * Props for the ViewAccountForm component.
 */
interface ViewAccountFormProps {
  readonly account: Account;
  readonly redirectUrl: string;
}

/**
 * Displays the read-only account workspace view for a selected account.
 */
const ViewAccountForm = function ({
  account,
  redirectUrl,
}: ViewAccountFormProps): JSX.Element {
  return (
    <Stack spacing={3} sx={{ width: "100%", maxWidth: 1200 }}>
      <Stack
        direction={{ xs: "column", sm: "row" }}
        spacing={1.5}
        justifyContent="space-between"
        alignItems={{ xs: "stretch", sm: "center" }}
      >
        <Typography variant="h5">View Account</Typography>
        <Stack direction="row" spacing={1.25} flexWrap="wrap" useFlexGap>
          <UpdateAccountForm account={account} redirectUrl={redirectUrl} />
          <DeleteAccountForm account={account} redirectUrl={redirectUrl} />
        </Stack>
      </Stack>
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
