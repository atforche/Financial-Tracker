"use client";

import type { FundBalanceEvent, FundWithBalance } from "@/funds/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import DeleteFundForm from "@/funds/workspace/DeleteFundForm";
import FundBalanceEventsFrame from "@/funds/workspace/FundBalanceEventsFrame";
import FundSummaryFrame from "@/funds/workspace/FundSummaryFrame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import { Stack } from "@mui/material";
import UpdateFundForm from "@/funds/workspace/UpdateFundForm";

/**
 * Props for the ViewFundForm component.
 */
interface ViewFundFormProps {
  readonly fund: FundWithBalance;
  readonly redirectUrl: string;
  readonly recentBalanceEvents: FundBalanceEvent[];
  readonly recentBalanceEventCount: number;
  readonly addTransactionHref: string;
}

/**
 * Displays the read-only fund workspace view for a selected fund.
 */
const ViewFundForm = function ({
  fund,
  redirectUrl,
  recentBalanceEvents,
  recentBalanceEventCount,
  addTransactionHref,
}: ViewFundFormProps): JSX.Element {
  return (
    <ConstrainedContent maxWidth={1200}>
      <PageLayout>
        <FundSummaryFrame
          fund={fund}
          headerContent={
            <Stack direction="row" spacing={1.25} flexWrap="wrap" useFlexGap>
              <UpdateFundForm fund={fund} redirectUrl={redirectUrl} />
              <DeleteFundForm fund={fund} redirectUrl={redirectUrl} />
            </Stack>
          }
        />
        <FundBalanceEventsFrame
          data={recentBalanceEvents}
          totalCount={recentBalanceEventCount}
          addTransactionHref={addTransactionHref}
        />
      </PageLayout>
    </ConstrainedContent>
  );
};

export default ViewFundForm;
