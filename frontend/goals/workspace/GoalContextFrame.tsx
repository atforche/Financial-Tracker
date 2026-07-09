import {
  type AssignmentGoal,
  type SpendingGoal,
  formatAssignmentGoalType,
  formatSpendingGoalType,
} from "@/goals/types";
import { Box, Stack } from "@mui/material";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import UpdateGoalForm from "@/goals/workspace/UpdateGoalForm";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the GoalContextFrame component.
 */
interface GoalContextFrameProps {
  readonly assignmentGoal: AssignmentGoal;
  readonly spendingGoal: SpendingGoal;
  readonly redirectUrl: string;
}

/** Displays the read-only context for the paired goals of one fund and period. */
const GoalContextFrame = function ({
  assignmentGoal,
  spendingGoal,
  redirectUrl,
}: GoalContextFrameProps): JSX.Element {
  return (
    <Frame
      title="Goal Context"
      headerContent={
        <UpdateGoalForm
          assignmentGoal={assignmentGoal}
          spendingGoal={spendingGoal}
          redirectUrl={redirectUrl}
        />
      }
    >
      <Stack spacing={2.5}>
        <Box
          sx={{
            display: "grid",
            gap: 2,
            gridTemplateColumns:
              "repeat(auto-fit, minmax(min(100%, 220px), 1fr))",
          }}
        >
          <TransactionDisplayField
            label="Accounting Period"
            value={assignmentGoal.accountingPeriodName ?? "Onboarded"}
          />
          <TransactionDisplayField
            label="Fund"
            value={assignmentGoal.fundName}
          />
        </Box>
        <Box
          sx={{
            display: "grid",
            gap: 2,
            gridTemplateColumns:
              "repeat(auto-fit, minmax(min(100%, 220px), 1fr))",
          }}
        >
          <TransactionDisplayField
            label="Assignment Goal Type"
            value={formatAssignmentGoalType(assignmentGoal.type)}
          />
          <TransactionDisplayField
            label="Assignment Goal Amount"
            value={formatCurrency(assignmentGoal.goalAmount)}
          />
          <TransactionDisplayField
            label="Remaining To Assign"
            value={formatCurrency(assignmentGoal.remainingAmountToAssign)}
          />
          <TransactionDisplayField
            label="Spending Goal Type"
            value={formatSpendingGoalType(spendingGoal.type)}
          />
          <TransactionDisplayField
            label="Remaining To Spend"
            value={formatCurrency(spendingGoal.remainingAmountToSpend)}
          />
        </Box>
      </Stack>
    </Frame>
  );
};

export default GoalContextFrame;
