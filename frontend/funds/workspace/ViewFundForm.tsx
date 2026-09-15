"use client";

import { Box, Button, Stack, Tab, Tabs } from "@mui/material";
import type {
  FundBalanceEvent,
  FundBalanceSummaryByDate,
  FundWithBalance,
} from "@/funds/types";
import { type JSX, useState } from "react";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import CurrentFundBalanceFrame from "@/funds/workspace/CurrentFundBalanceFrame";
import DeleteFundForm from "@/funds/workspace/DeleteFundForm";
import FundBalanceEventsFrame from "@/funds/workspace/FundBalanceEventsFrame";
import FundSummaryFrame from "@/funds/workspace/FundSummaryFrame";
import PageLayout from "@/framework/view/PageLayout";
import RecentBalanceActivity from "@/balance-events/RecentBalanceActivity";
import type { Route } from "next";
import UpdateFundForm from "@/funds/workspace/UpdateFundForm";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the ViewFundForm component.
 */
interface ViewFundFormProps {
  readonly fund: FundWithBalance;
  readonly redirectUrl: string;
  readonly deleteRedirectUrl: string;
  readonly recentBalanceEvents: FundBalanceEvent[];
  readonly recentBalanceEventCount: number;
  readonly recentActivityEvents: FundBalanceEvent[];
  readonly recentActivityBalances: readonly FundBalanceSummaryByDate[];
  readonly trendsHref: Route;
  readonly addTransactionHref: string;
}

/**
 * Displays the read-only fund workspace view for a selected fund.
 */
const ViewFundForm = function ({
  fund,
  redirectUrl,
  deleteRedirectUrl,
  recentBalanceEvents,
  recentBalanceEventCount,
  recentActivityEvents,
  recentActivityBalances,
  trendsHref,
  addTransactionHref,
}: ViewFundFormProps): JSX.Element {
  const canWrite = useWriteAccess();
  const [updateDialogOpen, setUpdateDialogOpen] = useState(false);
  const [activeView, setActiveView] = useState<"transactions" | "activity">(
    "transactions",
  );

  return (
    <ConstrainedContent maxWidth={1200}>
      <PageLayout>
        <FundSummaryFrame
          fund={fund}
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
                <DeleteFundForm fund={fund} redirectUrl={deleteRedirectUrl} />
              </Stack>
            )
          }
        />
        <CurrentFundBalanceFrame fund={fund} />
        <Box>
          <Tabs
            value={activeView}
            onChange={(_, value: "transactions" | "activity") => {
              setActiveView(value);
            }}
            aria-label="Fund workspace views"
            sx={{ mb: 2 }}
          >
            <Tab
              value="transactions"
              label="Transactions"
              id="fund-transactions-tab"
              aria-controls="fund-transactions-panel"
            />
            <Tab
              value="activity"
              label="Recent Activity"
              id="fund-activity-tab"
              aria-controls="fund-activity-panel"
            />
          </Tabs>
          <Box
            role="tabpanel"
            id="fund-transactions-panel"
            aria-labelledby="fund-transactions-tab"
            hidden={activeView !== "transactions"}
          >
            {activeView === "transactions" ? (
              <FundBalanceEventsFrame
                data={recentBalanceEvents}
                totalCount={recentBalanceEventCount}
                addTransactionHref={addTransactionHref}
              />
            ) : null}
          </Box>
          <Box
            role="tabpanel"
            id="fund-activity-panel"
            aria-labelledby="fund-activity-tab"
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
          <UpdateFundForm
            fund={fund}
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

export default ViewFundForm;
