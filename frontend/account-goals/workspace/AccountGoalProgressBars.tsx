import {
  type AccountGoal,
  AccountGoalEndingBalanceStatus,
  type AccountGoalProgress as AccountGoalProgressModel,
} from "@/account-goals/types";
import AccountGoalProgress from "@/account-goals/workspace/AccountGoalProgress";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import StringEntryField from "@/framework/forms/StringEntryField";
import { formatCurrency } from "@/framework/currencyHelpers";

interface AccountGoalProgressBarsProps {
  readonly accountGoal: AccountGoal;
  readonly progress: AccountGoalProgressModel;
  readonly showUnconfigured?: boolean;
  readonly showPositiveBalance?: boolean;
}

const displayAmount = (value: number | null | undefined): string =>
  value === null || value === undefined
    ? "Not configured"
    : formatCurrency(value);

/**
 * Displays Account Goal ending-balance progress, optionally including positive-balance progress.
 */
const AccountGoalProgressBars = function ({
  accountGoal,
  progress,
  showUnconfigured = false,
  showPositiveBalance = true,
}: AccountGoalProgressBarsProps): JSX.Element {
  const { endingBalance } = progress;
  return (
    <Stack spacing={2}>
      {showPositiveBalance ? (
        <AccountGoalProgress
          label="Positive Ending Balance"
          current={progress.positiveBalance.currentBalance}
          target={0}
          satisfied={progress.positiveBalance.isSatisfied}
          statusDescription={
            progress.positiveBalance.isSatisfied
              ? "Positive"
              : "Must be above $0"
          }
        />
      ) : null}
      {endingBalance?.minimumBalance !== null &&
      endingBalance?.minimumBalance !== undefined ? (
        <AccountGoalProgress
          label="Minimum Ending Balance"
          current={endingBalance.currentBalance}
          target={endingBalance.minimumBalance}
          satisfied={
            endingBalance.status !== AccountGoalEndingBalanceStatus.BelowMinimum
          }
        />
      ) : showUnconfigured ? (
        <StringEntryField
          label="Minimum Ending Balance"
          value={displayAmount(accountGoal.minimumEndingBalance)}
          setValue={null}
        />
      ) : null}
      {endingBalance?.maximumBalance !== null &&
      endingBalance?.maximumBalance !== undefined ? (
        <AccountGoalProgress
          label="Maximum Ending Balance"
          current={endingBalance.currentBalance}
          target={endingBalance.maximumBalance}
          satisfied={
            endingBalance.status !== AccountGoalEndingBalanceStatus.AboveMaximum
          }
          statusDescription={
            endingBalance.status === AccountGoalEndingBalanceStatus.AboveMaximum
              ? `${formatCurrency(endingBalance.amountAboveMaximum)} above maximum`
              : "Within maximum"
          }
        />
      ) : showUnconfigured ? (
        <StringEntryField
          label="Maximum Ending Balance"
          value={displayAmount(accountGoal.maximumEndingBalance)}
          setValue={null}
        />
      ) : null}
    </Stack>
  );
};

export default AccountGoalProgressBars;
