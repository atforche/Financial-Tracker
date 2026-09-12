"use client";
import type {
  FundGoal,
  FundGoalBalanceEvent,
  FundGoalProgress,
} from "@/fund-goals/types";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import type { BalanceTrendDateSummary } from "@/framework/charts/balanceTrendHelpers";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import Frame from "@/framework/view/Frame";
import FundGoalBalanceEventsFrame from "@/fund-goals/workspace/FundGoalBalanceEventsFrame";
import FundGoalContextFrame from "@/fund-goals/workspace/FundGoalContextFrame";
import FundGoalProgressOverview from "@/fund-goals/workspace/FundGoalProgressOverview";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import RecentBalanceActivity from "@/balance-events/RecentBalanceActivity";
import type { Route } from "next";

/**
 * Props for the ViewFundGoalForm component.
 */
interface ViewFundGoalFormProps {
  readonly fundGoal: FundGoal;
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly progress: FundGoalProgress;
  readonly redirectUrl: string;
  readonly recentBalanceEvents: FundGoalBalanceEvent[];
  readonly recentBalanceEventCount: number;
  readonly recentActivityBalances: readonly BalanceTrendDateSummary[];
  readonly periodOpeningBalance: number;
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
          accountingPeriod={props.accountingPeriod}
          redirectUrl={props.redirectUrl}
        />
        <Frame title="Progress">
          <FundGoalProgressOverview
            fundGoal={props.fundGoal}
            progress={props.progress}
          />
        </Frame>
        <RecentBalanceActivity
          data={[] as FundGoalBalanceEvent[]}
          dailyBalances={props.recentActivityBalances}
          periodOpeningBalance={props.periodOpeningBalance}
          trendsHref={props.trendsHref}
          getPreviousBalance={(event) =>
            event.previousTotals.amountAssigned -
            event.previousTotals.amountSpent
          }
          getNewBalance={(event) =>
            event.newTotals.amountAssigned - event.newTotals.amountSpent
          }
          title="Accounting Period Activity"
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
