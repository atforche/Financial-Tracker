"use client";

import {
  type FundBalanceEvent,
  FundBalanceEventSort,
  type FundWithBalance,
} from "@/funds/types";
import type { BalanceEventTotals } from "@/balance-events/types";
import type { BalanceTrendDateSummary } from "@/framework/charts/balanceTrendHelpers";
import BalanceWorkspaceView from "@/balance-events/BalanceWorkspaceView";
import type { JSX } from "react";
import type { Route } from "next";
import StringEntryField from "@/framework/forms/StringEntryField";
import UpdateFundForm from "@/funds/workspace/UpdateFundForm";
import deleteFund from "@/funds/workspace/deleteFund";

/**
 * Props for the ViewFundForm component.
 */
interface ViewFundFormProps {
  readonly fund: FundWithBalance;
  readonly redirectUrl: string;
  readonly deleteRedirectUrl: string;
  readonly recentBalanceEvents: FundBalanceEvent[];
  readonly recentBalanceEventCount: number;
  readonly activityTotals: BalanceEventTotals;
  readonly recentActivityBalances: readonly BalanceTrendDateSummary[];
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
  ...viewProps
}: ViewFundFormProps): JSX.Element {
  return (
    <BalanceWorkspaceView
      {...viewProps}
      entityName="fund"
      name={fund.name}
      summaryFields={
        <>
          <StringEntryField
            label="Name"
            value={fund.name}
            setValue={null}
            errorMessage={null}
          />
          <StringEntryField
            label="Description"
            value={fund.description}
            setValue={null}
            errorMessage={null}
          />
        </>
      }
      postedBalance={fund.currentBalance.postedBalance}
      pendingAmount={fund.pendingAmount}
      deleteAction={deleteFund}
      deletePayload={{ fundId: fund.id, redirectUrl: deleteRedirectUrl }}
      renderUpdateForm={(onClose) => (
        <UpdateFundForm
          fund={fund}
          redirectUrl={redirectUrl}
          onClose={onClose}
        />
      )}
      balanceEventSort={FundBalanceEventSort}
      emptyBalanceEventsDescription="Create a transaction for this fund to start building its balance history."
    />
  );
};

export default ViewFundForm;
