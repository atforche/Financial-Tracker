"use client";
import type {
  FundGoal,
  FundGoalBalanceEvent,
  FundGoalProgress,
} from "@/fund-goals/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import FundGoalBalanceEventsFrame from "@/fund-goals/workspace/FundGoalBalanceEventsFrame";
import FundGoalContextFrame from "@/fund-goals/workspace/FundGoalContextFrame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";

/**
 * Props for the ViewFundGoalForm component.
 */
interface ViewFundGoalFormProps {
  readonly fundGoal: FundGoal;
  readonly progress: FundGoalProgress;
  readonly redirectUrl: string;
  readonly recentBalanceEvents: FundGoalBalanceEvent[];
  readonly recentBalanceEventCount: number;
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
