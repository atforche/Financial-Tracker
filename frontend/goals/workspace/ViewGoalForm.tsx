"use client";
import type { FundPlan, FundPlanBalanceEvent } from "@/goals/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import GoalBalanceEventsFrame from "@/goals/workspace/GoalBalanceEventsFrame";
import GoalContextFrame from "@/goals/workspace/GoalContextFrame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";

/**
 * Props for the ViewGoalForm component.
 */
interface ViewGoalFormProps {
  readonly fundPlan: FundPlan;
  readonly redirectUrl: string;
  readonly recentBalanceEvents: FundPlanBalanceEvent[];
  readonly recentBalanceEventCount: number;
  readonly addTransactionHref: string;
  readonly accountingPeriodId: string;
  readonly fundId: string;
}

/**
 * Displays paired goal details, recent events, and a dialog-backed edit action.
 */
const ViewGoalForm = function (props: ViewGoalFormProps): JSX.Element {
  return (
    <ConstrainedContent maxWidth={1200}>
      <PageLayout>
        <GoalContextFrame
          fundPlan={props.fundPlan}
          redirectUrl={props.redirectUrl}
        />
        <GoalBalanceEventsFrame
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
export default ViewGoalForm;
