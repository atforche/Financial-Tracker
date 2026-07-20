import {
  EndingBalanceStatus,
  type FundPlan,
  type FundPlanProgress as FundPlanProgressModel,
  FundedBalanceStatus,
} from "@/fund-plans/types";
import FundPlanProgress from "@/fund-plans/workspace/FundPlanProgress";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import StringEntryField from "@/framework/forms/StringEntryField";
import { formatCurrency } from "@/framework/currencyHelpers";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";

/**
 * Props for the FundPlanProgressBars component.
 */
interface FundPlanProgressBarsProps {
  readonly fundPlan?: FundPlan;
  readonly progress: FundPlanProgressModel;
  readonly showUnconfigured?: boolean;
}

const displayAmount = (value: number | null | undefined): string =>
  value === null || value === undefined
    ? "Not configured"
    : formatCurrency(value);

/**
 * Displays progress bars for each configured Funding Plan metric.
 */
const FundPlanProgressBars = function ({
  fundPlan,
  progress,
  showUnconfigured = false,
}: FundPlanProgressBarsProps): JSX.Element {
  return (
    <Stack spacing={2}>
      {isNotNullOrUndefined(fundPlan?.regularContribution) &&
      progress.contribution ? (
        <FundPlanProgress
          label="Regular Monthly Contribution"
          current={progress.contribution.assignedAmount}
          target={progress.contribution.targetAmount}
          satisfied={progress.contribution.isSatisfied}
        />
      ) : showUnconfigured ? (
        <StringEntryField
          label="Regular Monthly Contribution"
          value={displayAmount(fundPlan?.regularContribution)}
          setValue={null}
        />
      ) : null}
      {progress.fundedBalance?.minimumBalance !== null &&
      progress.fundedBalance?.minimumBalance !== undefined ? (
        <FundPlanProgress
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
          value={displayAmount(fundPlan?.minimumFundedBalance)}
          setValue={null}
        />
      ) : null}
      {progress.fundedBalance?.maximumBalance !== null &&
      progress.fundedBalance?.maximumBalance !== undefined ? (
        <FundPlanProgress
          label="Maximum Funded Amount"
          current={progress.fundedBalance.balance}
          target={progress.fundedBalance.maximumBalance}
          satisfied={
            progress.fundedBalance.status !== FundedBalanceStatus.AboveMaximum
          }
        />
      ) : showUnconfigured ? (
        <StringEntryField
          label="Maximum Funded Amount"
          value={displayAmount(fundPlan?.maximumFundedBalance)}
          setValue={null}
        />
      ) : null}
      {progress.endingBalance ? (
        <FundPlanProgress
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
          value={displayAmount(fundPlan?.targetEndingBalance)}
          setValue={null}
        />
      ) : null}
    </Stack>
  );
};
export default FundPlanProgressBars;
