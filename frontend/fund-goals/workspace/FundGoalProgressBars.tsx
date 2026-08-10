import {
  EndingBalanceStatus,
  type FundGoal,
  type FundGoalProgress as FundGoalProgressModel,
  FundedBalanceStatus,
} from "@/fund-goals/types";
import FundGoalAvailableBalance from "@/fund-goals/workspace/FundGoalAvailableBalance";
import FundGoalProgress from "@/fund-goals/workspace/FundGoalProgress";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import StringEntryField from "@/framework/forms/StringEntryField";
import { formatCurrency } from "@/framework/currencyHelpers";
import { isMaximumFundedBalanceSatisfied } from "@/fund-goals/helpers";
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
      {isNotNullOrUndefined(fundGoal.regularContribution) &&
      progress.contribution ? (
        <FundGoalProgress
          label="Regular Monthly Contribution"
          current={progress.contribution.assignedAmount}
          target={progress.contribution.targetAmount}
          satisfied={progress.contribution.isSatisfied}
        />
      ) : showUnconfigured ? (
        <StringEntryField
          label="Regular Monthly Contribution"
          value={displayAmount(fundGoal.regularContribution)}
          setValue={null}
        />
      ) : null}
      {progress.fundedBalance?.minimumBalance !== null &&
      progress.fundedBalance?.minimumBalance !== undefined ? (
        <FundGoalProgress
          label="Minimum Funded Amount"
          current={progress.fundedBalance.balance}
          target={progress.fundedBalance.minimumBalance}
          satisfied={
            progress.fundedBalance.status !== FundedBalanceStatus.BelowMinimum
          }
        />
      ) : showUnconfigured ? (
        <StringEntryField
          label="Minimum Funded Amount"
          value={displayAmount(fundGoal.minimumFundedBalance)}
          setValue={null}
        />
      ) : null}
      {progress.fundedBalance?.maximumBalance !== null &&
      progress.fundedBalance?.maximumBalance !== undefined ? (
        <FundGoalProgress
          label="Maximum Funded Amount"
          current={progress.fundedBalance.balance}
          target={progress.fundedBalance.maximumBalance}
          satisfied={isMaximumFundedBalanceSatisfied(progress.fundedBalance)}
        />
      ) : showUnconfigured ? (
        <StringEntryField
          label="Maximum Funded Amount"
          value={displayAmount(fundGoal.maximumFundedBalance)}
          setValue={null}
        />
      ) : null}
      {progress.endingBalance ? (
        <FundGoalProgress
          label="Target Ending Balance"
          current={progress.endingBalance.currentBalance}
          target={progress.endingBalance.targetBalance}
          satisfied={
            progress.endingBalance.status === EndingBalanceStatus.AtTarget
          }
        />
      ) : showUnconfigured ? (
        <StringEntryField
          label="Target Ending Balance"
          value={displayAmount(fundGoal.targetEndingBalance)}
          setValue={null}
        />
      ) : null}
    </Stack>
  );
};
export default FundGoalProgressBars;
