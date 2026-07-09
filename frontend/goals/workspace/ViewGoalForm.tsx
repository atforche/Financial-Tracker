"use client";

import type {
  AssignmentGoal,
  GoalWorkspaceBalanceEvent,
  SpendingGoal,
} from "@/goals/types";
import GoalBalanceEventsFrame from "@/goals/workspace/GoalBalanceEventsFrame";
import GoalContextFrame from "@/goals/workspace/GoalContextFrame";
import type { JSX } from "react";
import { Stack } from "@mui/material";

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
    <Stack spacing={3} sx={{ width: "100%", maxWidth: 1200 }}>
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
    </Stack>
  );
};

export default ViewGoalForm;
