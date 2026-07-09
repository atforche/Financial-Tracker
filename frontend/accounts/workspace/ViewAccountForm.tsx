"use client";

import type { Account, AccountWorkspaceBalanceEvent } from "@/accounts/types";
import AccountBalanceEventsFrame from "@/accounts/workspace/AccountBalanceEventsFrame";
import AccountSummaryFrame from "@/accounts/workspace/AccountSummaryFrame";
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
      <AccountSummaryFrame
        account={account}
        headerContent={
          <Stack direction="row" spacing={1.25} flexWrap="wrap" useFlexGap>
            <UpdateAccountForm account={account} redirectUrl={redirectUrl} />
            <DeleteAccountForm account={account} redirectUrl={redirectUrl} />
          </Stack>
        }
      />
      <AccountBalanceEventsFrame
        data={recentBalanceEvents}
        totalCount={recentBalanceEventCount}
        addTransactionHref={addTransactionHref}
      />
    </Stack>
  );
};

export default ViewAccountForm;
