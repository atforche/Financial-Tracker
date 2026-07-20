import Frame from "@/framework/view/Frame";
import type { FundPlan } from "@/goals/types";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";
import UpdateGoalForm from "@/goals/workspace/UpdateGoalForm";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the GoalContextFrame component.
 */
interface GoalContextFrameProps {
  readonly fundPlan: FundPlan;
  readonly redirectUrl: string;
}

/**
 * Formats a numeric value as a currency string, or returns "Not configured" if the value is null or undefined.
 */
const displayAmount = (value: number | null | undefined): string =>
  value === null || value === undefined
    ? "Not configured"
    : formatCurrency(value);

/**
 * Component that displays the context of a goal within a frame, including its details and an update form.
 */
const GoalContextFrame = function ({
  fundPlan,
  redirectUrl,
}: GoalContextFrameProps): JSX.Element {
  return (
    <Frame
      title="Goal Context"
      headerContent={
        <UpdateGoalForm fundPlan={fundPlan} redirectUrl={redirectUrl} />
      }
    >
      <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
        <StringEntryField
          label="Accounting Period"
          value={fundPlan.accountingPeriod?.name ?? "Onboarded"}
          setValue={null}
        />
        <StringEntryField
          label="Fund"
          value={fundPlan.fund.name}
          setValue={null}
        />
        <StringEntryField
          label="Regular Monthly Contribution"
          value={displayAmount(fundPlan.regularContribution)}
          setValue={null}
        />
        <StringEntryField
          label="Minimum Funded Amount"
          value={displayAmount(fundPlan.minimumFundedBalance)}
          setValue={null}
        />
        <StringEntryField
          label="Maximum Funded Amount"
          value={displayAmount(fundPlan.maximumFundedBalance)}
          setValue={null}
        />
        <StringEntryField
          label="Target Ending Balance"
          value={displayAmount(fundPlan.targetEndingBalance)}
          setValue={null}
        />
      </ResponsiveGrid>
    </Frame>
  );
};
export default GoalContextFrame;
