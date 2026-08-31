import {
  type FundGoal,
  FundGoalEndingBalanceStatus,
  type FundGoalProgress as FundGoalProgressModel,
} from "@/fund-goals/types";
import FundGoalAvailableBalance from "@/fund-goals/workspace/FundGoalAvailableBalance";
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
  readonly showAvailableBalance?: boolean;
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
  showAvailableBalance = true,
  showUnconfigured = false,
}: FundGoalProgressBarsProps): JSX.Element {
  return (
    <Stack spacing={2}>
      {showAvailableBalance ? (
        <FundGoalAvailableBalance
          availableBalance={progress.availableBalance}
        />
      ) : null}
      {isNotNullOrUndefined(fundGoal.plannedMonthlyContribution) &&
      progress.contribution ? (
        <FundGoalProgress
          label="Expected Contribution"
          current={progress.contribution.assignedAmount}
          target={progress.contribution.expectedAmount}
          satisfied={progress.contribution.isSatisfied}
        />
      ) : showUnconfigured ? (
        <StringEntryField
          label="Planned Monthly Contribution"
          value={displayAmount(fundGoal.plannedMonthlyContribution)}
          setValue={null}
        />
      ) : null}
      {progress.endingBalance?.minimumBalance !== null &&
      progress.endingBalance?.minimumBalance !== undefined ? (
        <FundGoalProgress
          label="Minimum Ending Balance"
          current={progress.endingBalance.currentBalance}
          target={progress.endingBalance.minimumBalance}
          satisfied={
            progress.endingBalance.status !==
            FundGoalEndingBalanceStatus.BelowMinimum
          }
        />
      ) : showUnconfigured ? (
        <StringEntryField
          label="Minimum Ending Balance"
          value={displayAmount(fundGoal.minimumEndingBalance)}
          setValue={null}
        />
      ) : null}
      {progress.endingBalance?.maximumBalance !== null &&
      progress.endingBalance?.maximumBalance !== undefined ? (
        <FundGoalProgress
          label="Maximum Ending Balance"
          current={progress.endingBalance.currentBalance}
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
