import type {
  AccountGoal,
  AccountGoalProgress as AccountGoalProgressModel,
} from "@/account-goals/types";
import AccountGoalProgressBars from "@/account-goals/workspace/AccountGoalProgressBars";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";
import UpdateAccountGoalForm from "@/account-goals/workspace/UpdateAccountGoalForm";

interface AccountGoalContextFrameProps {
  readonly accountGoal: AccountGoal;
  readonly progress: AccountGoalProgressModel;
  readonly redirectUrl: string;
  readonly isReadOnly: boolean;
}

/**
 * Displays an Account Goal's account, period, progress, and configuration action.
 */
const AccountGoalContextFrame = function ({
  accountGoal,
  progress,
  redirectUrl,
  isReadOnly,
}: AccountGoalContextFrameProps): JSX.Element {
  return (
    <Frame
      title="Account Goal"
      headerContent={
        isReadOnly ? null : (
          <UpdateAccountGoalForm
            accountGoal={accountGoal}
            redirectUrl={redirectUrl}
          />
        )
      }
    >
      <ResponsiveGrid columns={{ xs: 1 }} spacing={2}>
        <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
          <StringEntryField
            label="Accounting Period"
            value={accountGoal.accountingPeriod?.name ?? "Onboarded"}
            setValue={null}
          />
          <StringEntryField
            label="Account"
            value={accountGoal.account.name}
            setValue={null}
          />
        </ResponsiveGrid>
        <AccountGoalProgressBars
          accountGoal={accountGoal}
          progress={progress}
          showUnconfigured
        />
      </ResponsiveGrid>
    </Frame>
  );
};

export default AccountGoalContextFrame;
