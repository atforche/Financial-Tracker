"use client";

import type { Fund, FundWorkspaceBalanceEvent } from "@/funds/types";
import DeleteFundForm from "@/funds/workspace/DeleteFundForm";
import FundBalanceEventsFrame from "@/funds/workspace/FundBalanceEventsFrame";
import FundSummaryFrame from "@/funds/workspace/FundSummaryFrame";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import UpdateFundForm from "@/funds/workspace/UpdateFundForm";

/**
 * Props for the ViewFundForm component.
 */
interface ViewFundFormProps {
  readonly fund: Fund;
  readonly redirectUrl: string;
  readonly recentBalanceEvents: FundWorkspaceBalanceEvent[];
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
    <Stack spacing={3} sx={{ width: "100%", maxWidth: 1200 }}>
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
    </Stack>
  );
};

export default ViewFundForm;
