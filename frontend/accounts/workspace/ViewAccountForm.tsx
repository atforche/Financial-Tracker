"use client";

import type { Account, AccountWorkspaceBalanceEvent } from "@/accounts/types";
import AccountBalanceEventsFrame from "@/accounts/workspace/AccountBalanceEventsFrame";
import AccountCurrentBalanceFrame from "@/accounts/workspace/AccountCurrentBalanceFrame";
import AccountDetailsFrame from "@/accounts/workspace/AccountDetailsFrame";
import DeleteAccountForm from "@/accounts/workspace/DeleteAccountForm";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import UpdateAccountForm from "@/accounts/workspace/UpdateAccountForm";

/**
 * Props for the ViewAccountForm component.
 */
interface ViewAccountFormProps {
  readonly account: Account;
  readonly redirectUrl: string;
  readonly recentBalanceEvents: AccountWorkspaceBalanceEvent[];
  readonly recentBalanceEventCount: number;
  readonly addTransactionHref: string;
}

/**
 * Displays the read-only account workspace view for a selected account.
 */
const ViewAccountForm = function ({
  account,
  redirectUrl,
  recentBalanceEvents,
  recentBalanceEventCount,
  addTransactionHref,
}: ViewAccountFormProps): JSX.Element {
  return (
    <Stack spacing={3} sx={{ width: "100%", maxWidth: 1200 }}>
      <AccountDetailsFrame
        color="info"
        name={account.name}
        setName={null}
        nameErrorMessage={null}
        accountType={account.type}
        setAccountType={null}
        accountTypeErrorMessage={null}
        headerContent={
          <Stack direction="row" spacing={1.25} flexWrap="wrap" useFlexGap>
            <UpdateAccountForm account={account} redirectUrl={redirectUrl} />
            <DeleteAccountForm account={account} redirectUrl={redirectUrl} />
          </Stack>
        }
      />
      <AccountCurrentBalanceFrame account={account} />
      <AccountBalanceEventsFrame
        data={recentBalanceEvents}
        totalCount={recentBalanceEventCount}
        addTransactionHref={addTransactionHref}
      />
    </Stack>
  );
};

export default ViewAccountForm;
