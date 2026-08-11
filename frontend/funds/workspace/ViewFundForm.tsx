"use client";

import { Button, Stack } from "@mui/material";
import type {
  FundBalanceEvent,
  FundBalanceSummaryByDate,
  FundWithBalance,
} from "@/funds/types";
import { type JSX, useState } from "react";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
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
        <RecentBalanceActivity
          data={recentActivityEvents}
          dailyBalances={recentActivityBalances}
          trendsHref={trendsHref}
          getPreviousBalance={(event) => event.previousBalance.postedBalance}
          getNewBalance={(event) => event.newBalance.postedBalance}
        />
        <FundBalanceEventsFrame
          data={recentBalanceEvents}
          totalCount={recentBalanceEventCount}
          addTransactionHref={addTransactionHref}
        />
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
