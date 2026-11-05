import type { AccountGoal, AccountGoalProgress } from "@/account-goals/types";
import { Stack, Typography } from "@mui/material";
import ContentSurface from "@/framework/view/ContentSurface";
import GoalEndingBalanceSection from "@/accounting-periods/workspace/GoalEndingBalanceSection";
import type { JSX } from "react";
import UpdateAccountGoalForm from "@/account-goals/workspace/UpdateAccountGoalForm";

/**
 * Props for the PeriodAccountGoalCard component.
 */
interface PeriodAccountGoalCardProps {
  readonly goal: AccountGoal;
  readonly progress: AccountGoalProgress;
  readonly isOpen: boolean;
  readonly redirectUrl: string;
}

/**
 * Shows whether an account's balance ended a period where its goal says it should.
 */
const PeriodAccountGoalCard = function ({
  goal,
  progress,
  isOpen,
  redirectUrl,
}: PeriodAccountGoalCardProps): JSX.Element {
  return (
    <ContentSurface>
      <Stack spacing={2}>
        <Stack
          direction="row"
          spacing={2}
          sx={{ alignItems: "center", justifyContent: "space-between" }}
        >
          <Typography variant="h5">Goal</Typography>
          {isOpen ? (
            <UpdateAccountGoalForm
              accountGoal={goal}
              redirectUrl={redirectUrl}
            />
          ) : null}
        </Stack>
        <GoalEndingBalanceSection
          {...progress.endingBalance}
          balance={progress.endingBalance.currentBalance}
        />
      </Stack>
    </ContentSurface>
  );
};

export default PeriodAccountGoalCard;
