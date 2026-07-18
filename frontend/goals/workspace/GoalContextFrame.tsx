import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import {
  formatAssignmentGoalType,
  formatSpendingGoalType,
} from "@/goals/helpers";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import { Stack } from "@mui/material";
import StringEntryField from "@/framework/forms/StringEntryField";
import UpdateGoalForm from "@/goals/workspace/UpdateGoalForm";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the GoalContextFrame component.
 */
interface GoalContextFrameProps {
  readonly assignmentGoal: AssignmentGoal;
  readonly spendingGoal: SpendingGoal;
  readonly redirectUrl: string;
}

/**
 * Displays the read-only context for the paired goals of one fund and period.
 */
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
        <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
          <StringEntryField
            label="Accounting Period"
            value={assignmentGoal.accountingPeriod?.name ?? "Onboarded"}
            setValue={null}
          />
          <StringEntryField
            label="Fund"
            value={assignmentGoal.fund.name}
            setValue={null}
          />
        </ResponsiveGrid>
        <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
          <StringEntryField
            label="Assignment Goal Type"
            value={formatAssignmentGoalType(assignmentGoal.type)}
            setValue={null}
          />
          <StringEntryField
            label="Assignment Goal Amount"
            value={formatCurrency(assignmentGoal.goalAmount)}
            setValue={null}
          />
          <StringEntryField
            label="Remaining To Assign"
            value={formatCurrency(assignmentGoal.remainingAmountToAssign)}
            setValue={null}
          />
          <StringEntryField
            label="Spending Goal Type"
            value={formatSpendingGoalType(spendingGoal.type)}
            setValue={null}
          />
          <StringEntryField
            label="Remaining To Spend"
            value={formatCurrency(spendingGoal.remainingAmountToSpend)}
            setValue={null}
          />
        </ResponsiveGrid>
      </Stack>
    </Frame>
  );
};

export default GoalContextFrame;
