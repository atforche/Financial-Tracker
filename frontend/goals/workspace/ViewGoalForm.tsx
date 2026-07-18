"use client";

import type {
  AssignmentGoal,
  GoalWorkspaceBalanceEvent,
  SpendingGoal,
} from "@/goals/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import GoalBalanceEventsFrame from "@/goals/workspace/GoalBalanceEventsFrame";
import GoalContextFrame from "@/goals/workspace/GoalContextFrame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";

/**
 * Props for the ViewGoalForm component.
 */
interface ViewGoalFormProps {
  readonly assignmentGoal: AssignmentGoal;
  readonly spendingGoal: SpendingGoal;
  readonly redirectUrl: string;
  readonly recentBalanceEvents: GoalWorkspaceBalanceEvent[];
  readonly recentBalanceEventCount: number;
  readonly addTransactionHref: string;
  readonly accountingPeriodId: string;
  readonly fundId: string;
}

/** Displays paired goal details, recent events, and a dialog-backed edit action. */
const ViewGoalForm = function ({
  assignmentGoal,
  spendingGoal,
  redirectUrl,
  recentBalanceEvents,
  recentBalanceEventCount,
  addTransactionHref,
  accountingPeriodId,
  fundId,
}: ViewGoalFormProps): JSX.Element {
  return (
    <ConstrainedContent maxWidth={1200}>
      <PageLayout>
        <GoalContextFrame
          assignmentGoal={assignmentGoal}
          spendingGoal={spendingGoal}
          redirectUrl={redirectUrl}
        />
        <GoalBalanceEventsFrame
          data={recentBalanceEvents}
          totalCount={recentBalanceEventCount}
          addTransactionHref={addTransactionHref}
          accountingPeriodId={accountingPeriodId}
          fundId={fundId}
        />
      </PageLayout>
    </ConstrainedContent>
  );
};

export default ViewGoalForm;
