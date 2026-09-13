"use client";

import type {
  AccountBalanceEvent,
  AccountBalanceSummaryByDate,
  AccountWithBalance,
} from "@/accounts/types";
import { Box, Button, Stack, Tab, Tabs } from "@mui/material";
import { type JSX, useState } from "react";
import AccountBalanceEventsFrame from "@/accounts/workspace/AccountBalanceEventsFrame";
import AccountSummaryFrame from "@/accounts/workspace/AccountSummaryFrame";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import CurrentBalanceFrame from "@/accounts/workspace/CurrentBalanceFrame";
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
  const [activeView, setActiveView] = useState<"transactions" | "activity">(
    "transactions",
  );

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
        <CurrentBalanceFrame account={account} />
        <Box>
          <Tabs
            value={activeView}
            onChange={(_, value: "transactions" | "activity") => {
              setActiveView(value);
            }}
            aria-label="Account workspace views"
            sx={{ mb: 2 }}
          >
            <Tab
              value="transactions"
              label="Transactions"
              id="account-transactions-tab"
              aria-controls="account-transactions-panel"
            />
            <Tab
              value="activity"
              label="Recent Activity"
              id="account-activity-tab"
              aria-controls="account-activity-panel"
            />
          </Tabs>
          <Box
            role="tabpanel"
            id="account-transactions-panel"
            aria-labelledby="account-transactions-tab"
            hidden={activeView !== "transactions"}
          >
            {activeView === "transactions" ? (
              <AccountBalanceEventsFrame
                data={recentBalanceEvents}
                totalCount={recentBalanceEventCount}
                addTransactionHref={addTransactionHref}
              />
            ) : null}
          </Box>
          <Box
            role="tabpanel"
            id="account-activity-panel"
            aria-labelledby="account-activity-tab"
            hidden={activeView !== "activity"}
          >
            {activeView === "activity" ? (
              <RecentBalanceActivity
                summaryFirst
                title="Balance Activity"
                data={recentActivityEvents}
                dailyBalances={recentActivityBalances}
                trendsHref={trendsHref}
                getPreviousBalance={(event) =>
                  event.previousBalance.postedBalance
                }
                getNewBalance={(event) => event.newBalance.postedBalance}
              />
            ) : null}
          </Box>
        </Box>
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
