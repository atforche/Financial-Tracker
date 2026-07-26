"use client";

import type { AccountBalanceEvent, AccountWithBalance } from "@/accounts/types";
import { Button, Stack } from "@mui/material";
import { type JSX, useState } from "react";
import AccountBalanceEventsFrame from "@/accounts/workspace/AccountBalanceEventsFrame";
import AccountSummaryFrame from "@/accounts/workspace/AccountSummaryFrame";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import DeleteAccountForm from "@/accounts/workspace/DeleteAccountForm";
import PageLayout from "@/framework/view/PageLayout";
import UpdateAccountForm from "@/accounts/workspace/UpdateAccountForm";

/**
 * Props for the ViewAccountForm component.
 */
interface ViewAccountFormProps {
  readonly account: AccountWithBalance;
  readonly redirectUrl: string;
  readonly deleteRedirectUrl: string;
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
  deleteRedirectUrl,
  recentBalanceEvents,
  recentBalanceEventCount,
  addTransactionHref,
}: ViewAccountFormProps): JSX.Element {
  const [updateDialogOpen, setUpdateDialogOpen] = useState(false);

  return (
    <ConstrainedContent maxWidth={1200}>
      <PageLayout>
        <AccountSummaryFrame
          account={account}
          headerContent={
            <Stack direction="row" spacing={1.25} flexWrap="wrap" useFlexGap>
              <Button
                variant="contained"
                onClick={() => {
                  setUpdateDialogOpen(true);
                }}
              >
                Edit
              </Button>
              <DeleteAccountForm
                account={account}
                redirectUrl={deleteRedirectUrl}
              />
            </Stack>
          }
        />
        <AccountBalanceEventsFrame
          data={recentBalanceEvents}
          totalCount={recentBalanceEventCount}
          addTransactionHref={addTransactionHref}
        />
        {updateDialogOpen ? (
          <UpdateAccountForm
            account={account}
            redirectUrl={redirectUrl}
            onClose={() => {
              setUpdateDialogOpen(false);
            }}
          />
        ) : null}
      </PageLayout>
    </ConstrainedContent>
  );
};

export default ViewAccountForm;
