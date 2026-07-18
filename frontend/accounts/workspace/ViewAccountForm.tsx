"use client";

import type { AccountBalanceEvent, AccountWithBalance } from "@/accounts/types";
import AccountBalanceEventsFrame from "@/accounts/workspace/AccountBalanceEventsFrame";
import AccountSummaryFrame from "@/accounts/workspace/AccountSummaryFrame";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import DeleteAccountForm from "@/accounts/workspace/DeleteAccountForm";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import { Stack } from "@mui/material";
import UpdateAccountForm from "@/accounts/workspace/UpdateAccountForm";

/**
 * Props for the ViewAccountForm component.
 */
interface ViewAccountFormProps {
  readonly account: AccountWithBalance;
  readonly redirectUrl: string;
  readonly recentBalanceEvents: AccountBalanceEvent[];
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
    <ConstrainedContent maxWidth={1200}>
      <PageLayout>
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
      </PageLayout>
    </ConstrainedContent>
  );
};

export default ViewAccountForm;
