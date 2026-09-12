import {
  type FundGoal,
  FundGoalEndingBalanceStatus,
  type FundGoalProgress as FundGoalProgressModel,
} from "@/fund-goals/types";
import FundGoalContributionAdjustment from "@/fund-goals/workspace/FundGoalContributionAdjustment";
import FundGoalEndingBalance from "@/fund-goals/workspace/FundGoalEndingBalance";
import FundGoalProgress from "@/fund-goals/workspace/FundGoalProgress";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import StringEntryField from "@/framework/forms/StringEntryField";
import { formatCurrency } from "@/framework/currencyHelpers";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";

/**
 * Props for the FundGoalProgressBars component.
 */
interface FundGoalProgressBarsProps {
  readonly fundGoal: FundGoal;
  readonly progress: FundGoalProgressModel;
  readonly showEndingBalance?: boolean;
  readonly showUnconfigured?: boolean;
}

const displayAmount = (value: number | null | undefined): string =>
  value === null || value === undefined
    ? "Not configured"
    : formatCurrency(value);

/**
 * Displays progress bars for each configured Fund Goal metric.
 */
const FundGoalProgressBars = function ({
  fundGoal,
  progress,
  showEndingBalance = true,
  showUnconfigured = false,
}: FundGoalProgressBarsProps): JSX.Element {
  return (
    <Stack spacing={2}>
      {showEndingBalance ? (
        <FundGoalEndingBalance
          endingBalance={progress.endingBalance.endingBalance}
        />
      ) : null}
      {isNotNullOrUndefined(fundGoal.plannedMonthlyContribution) &&
      progress.contribution ? (
        <Stack spacing={1}>
          <FundGoalContributionAdjustment
            plannedAmount={progress.contribution.plannedAmount}
            adjustmentAmount={
              progress.contribution.amountReducedByMaximumEndingBalance
            }
          />
          <FundGoalProgress
            label="Expected Contribution"
            current={progress.contribution.assignedAmount}
            target={progress.contribution.expectedAmount}
            satisfied={progress.contribution.isSatisfied}
          />
        </Stack>
      ) : showUnconfigured ? (
        <StringEntryField
          label="Planned Monthly Contribution"
          value={displayAmount(fundGoal.plannedMonthlyContribution)}
          setValue={null}
        />
      ) : null}
      <FundGoalProgress
        label="Minimum Ending Balance"
        current={progress.endingBalance.endingBalance}
        target={progress.endingBalance.minimumBalance}
        satisfied={
          progress.endingBalance.status !==
          FundGoalEndingBalanceStatus.BelowMinimum
        }
      />
      {progress.endingBalance.maximumBalance !== null &&
      progress.endingBalance.maximumBalance !== undefined ? (
        <FundGoalProgress
          label="Maximum Ending Balance"
          current={progress.endingBalance.endingBalance}
          target={progress.endingBalance.maximumBalance}
          satisfied={
            progress.endingBalance.status !==
            FundGoalEndingBalanceStatus.AboveMaximum
          }
          statusDescription={
            progress.endingBalance.status ===
            FundGoalEndingBalanceStatus.AboveMaximum
              ? `${formatCurrency(progress.endingBalance.amountAboveMaximum)} above maximum`
              : "Within maximum"
          }
        />
      ) : showUnconfigured ? (
        <StringEntryField
          label="Maximum Ending Balance"
          value={displayAmount(fundGoal.maximumEndingBalance)}
          setValue={null}
        />
      ) : null}
    </Stack>
  );
};
export default FundGoalProgressBars;
