"use client";

import type {
  AccountBalanceEvent,
  AccountBalanceSummaryByDate,
  AccountWithBalance,
} from "@/accounts/types";
import { Button, Stack } from "@mui/material";
import { type JSX, useState } from "react";
import AccountBalanceEventsFrame from "@/accounts/workspace/AccountBalanceEventsFrame";
import AccountSummaryFrame from "@/accounts/workspace/AccountSummaryFrame";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import DeleteAccountForm from "@/accounts/workspace/DeleteAccountForm";
import PageLayout from "@/framework/view/PageLayout";
import RecentBalanceActivity from "@/balance-events/RecentBalanceActivity";
import type { Route } from "next";
import UpdateAccountForm from "@/accounts/workspace/UpdateAccountForm";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the ViewAccountForm component.
 */
interface ViewAccountFormProps {
  readonly account: AccountWithBalance;
  readonly financialInstitutions: readonly string[];
  readonly redirectUrl: string;
  readonly deleteRedirectUrl: string;
  readonly recentBalanceEvents: AccountBalanceEvent[];
  readonly recentBalanceEventCount: number;
  readonly recentActivityEvents: AccountBalanceEvent[];
  readonly recentActivityBalances: readonly AccountBalanceSummaryByDate[];
  readonly trendsHref: Route;
  readonly addTransactionHref: string;
}

/**
 * Displays the read-only account workspace view for a selected account.
 */
const ViewAccountForm = function ({
  account,
  financialInstitutions,
  redirectUrl,
  deleteRedirectUrl,
  recentBalanceEvents,
  recentBalanceEventCount,
  recentActivityEvents,
  recentActivityBalances,
  trendsHref,
  addTransactionHref,
}: ViewAccountFormProps): JSX.Element {
  const canWrite = useWriteAccess();
  const [updateDialogOpen, setUpdateDialogOpen] = useState(false);

  return (
    <ConstrainedContent maxWidth={1200}>
      <PageLayout>
        <AccountSummaryFrame
          account={account}
          headerContent={
            !canWrite ? undefined : (
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
            )
          }
        />
        <RecentBalanceActivity
          data={recentActivityEvents}
          dailyBalances={recentActivityBalances}
          trendsHref={trendsHref}
          getPreviousBalance={(event) => event.previousBalance.postedBalance}
          getNewBalance={(event) => event.newBalance.postedBalance}
        />
        <AccountBalanceEventsFrame
          data={recentBalanceEvents}
          totalCount={recentBalanceEventCount}
          addTransactionHref={addTransactionHref}
        />
        {canWrite && updateDialogOpen ? (
          <UpdateAccountForm
            account={account}
            financialInstitutions={financialInstitutions}
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
