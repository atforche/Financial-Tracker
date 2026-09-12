import { Divider, Stack, Typography } from "@mui/material";
import type {
  FundGoal,
  FundGoalProgress as FundGoalProgressModel,
} from "@/fund-goals/types";
import FundGoalContributionAdjustment from "@/fund-goals/workspace/FundGoalContributionAdjustment";
import FundGoalEndingBalanceRange from "@/fund-goals/workspace/FundGoalEndingBalanceRange";
import FundGoalProgress from "@/fund-goals/workspace/FundGoalProgress";
import type { JSX } from "react";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";

interface FundGoalProgressOverviewProps {
  readonly fundGoal: FundGoal;
  readonly progress: FundGoalProgressModel;
}

/**
 * Shows the same contribution and ending-balance progress in both goal views.
 */
const FundGoalProgressOverview = function ({
  fundGoal,
  progress,
}: FundGoalProgressOverviewProps): JSX.Element {
  return (
    <Stack spacing={2}>
      {isNotNullOrUndefined(fundGoal.plannedMonthlyContribution) &&
      progress.contribution ? (
        <Stack spacing={2}>
          <FundGoalContributionAdjustment
            plannedAmount={progress.contribution.plannedAmount}
            adjustmentAmount={
              progress.contribution.amountReducedByMaximumEndingBalance
            }
            expectedAmount={progress.contribution.expectedAmount}
          />
          <Divider />
          <FundGoalProgress
            label="Expected Contribution"
            current={progress.contribution.assignedAmount}
            target={progress.contribution.expectedAmount}
            satisfied={progress.contribution.isSatisfied}
          />
        </Stack>
      ) : (
        <Typography variant="body2" color="text.secondary">
          No planned contribution configured.
        </Typography>
      )}
      <Divider />
      <FundGoalEndingBalanceRange endingBalance={progress.endingBalance} />
    </Stack>
  );
};

export default FundGoalProgressOverview;
