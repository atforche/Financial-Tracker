import type { AccountGoal } from "@/account-goals/types";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";
import UpdateAccountGoalForm from "@/account-goals/workspace/UpdateAccountGoalForm";
import { formatCurrency } from "@/framework/currencyHelpers";
import { isNullOrUndefined } from "@/framework/nullHelpers";

interface AccountGoalContextFrameProps {
  readonly accountGoal: AccountGoal;
  readonly redirectUrl: string;
  readonly isReadOnly: boolean;
}

/**
 * Displays an Account Goal's account, period, progress, and configuration action.
 */
const AccountGoalContextFrame = function ({
  accountGoal,
  redirectUrl,
  isReadOnly,
}: AccountGoalContextFrameProps): JSX.Element {
  return (
    <Frame
      title="Details"
      headerContent={
        isReadOnly ? null : (
          <UpdateAccountGoalForm
            accountGoal={accountGoal}
            redirectUrl={redirectUrl}
          />
        )
      }
    >
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
        <StringEntryField
          label="Minimum Ending Balance"
          value={formatCurrency(accountGoal.minimumEndingBalance)}
          setValue={null}
        />
        <StringEntryField
          label="Maximum Ending Balance"
          value={
            isNullOrUndefined(accountGoal.maximumEndingBalance)
              ? "Not configured"
              : formatCurrency(accountGoal.maximumEndingBalance)
          }
          setValue={null}
        />
      </ResponsiveGrid>
    </Frame>
  );
};

export default AccountGoalContextFrame;
