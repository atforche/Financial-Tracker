"use client";
import type {
  FundGoal,
  FundGoalBalanceEvent,
  FundGoalBalanceSummaryByDate,
  FundGoalProgress,
} from "@/fund-goals/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import FundGoalBalanceEventsFrame from "@/fund-goals/workspace/FundGoalBalanceEventsFrame";
import FundGoalContextFrame from "@/fund-goals/workspace/FundGoalContextFrame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import RecentBalanceActivity from "@/balance-events/RecentBalanceActivity";
import type { Route } from "next";

/**
 * Props for the ViewFundGoalForm component.
 */
interface ViewFundGoalFormProps {
  readonly fundGoal: FundGoal;
  readonly progress: FundGoalProgress;
  readonly redirectUrl: string;
  readonly recentBalanceEvents: FundGoalBalanceEvent[];
  readonly recentBalanceEventCount: number;
  readonly recentActivityEvents: FundGoalBalanceEvent[];
  readonly recentActivityBalances: FundGoalBalanceSummaryByDate[];
  readonly trendsHref: Route;
  readonly addTransactionHref: string;
  readonly accountingPeriodId: string;
  readonly fundId: string;
}

/**
 * Displays Fund Goal details, recent events, and an edit action.
 */
const ViewFundGoalForm = function (props: ViewFundGoalFormProps): JSX.Element {
  return (
    <ConstrainedContent maxWidth={1200}>
      <PageLayout>
        <FundGoalContextFrame
          fundGoal={props.fundGoal}
          progress={props.progress}
          redirectUrl={props.redirectUrl}
        />
        <RecentBalanceActivity
          data={props.recentActivityEvents}
          dailyBalances={props.recentActivityBalances}
          trendsHref={props.trendsHref}
          getPreviousBalance={(event) =>
            event.previousTotals.amountAssigned -
            event.previousTotals.amountSpent
          }
          getNewBalance={(event) =>
            event.newTotals.amountAssigned - event.newTotals.amountSpent
          }
          title="Recent Activity"
          balanceLabel="Fund Balance"
        />
        <FundGoalBalanceEventsFrame
          data={props.recentBalanceEvents}
          totalCount={props.recentBalanceEventCount}
          addTransactionHref={props.addTransactionHref}
          accountingPeriodId={props.accountingPeriodId}
          fundId={props.fundId}
        />
      </PageLayout>
    </ConstrainedContent>
  );
};
export default ViewFundGoalForm;
