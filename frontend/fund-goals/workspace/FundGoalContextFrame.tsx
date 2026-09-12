import Frame from "@/framework/view/Frame";
import type { FundGoal } from "@/fund-goals/types";
import type { JSX } from "react";
import ReadOnlyField from "@/framework/forms/ReadOnlyField";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import UpdateFundGoalForm from "@/fund-goals/workspace/UpdateFundGoalForm";
import { formatCurrency } from "@/framework/currencyHelpers";
import { isNullOrUndefined } from "@/framework/nullHelpers";

/**
 * Props for the FundGoalContextFrame component.
 */
interface FundGoalContextFrameProps {
  readonly fundGoal: FundGoal;
  readonly redirectUrl: string;
}

/**
 * Displays a Fund Goal's configured details and update action.
 */
const FundGoalContextFrame = function ({
  fundGoal,
  redirectUrl,
}: FundGoalContextFrameProps): JSX.Element {
  return (
    <Frame
      title="Details"
      headerContent={
        <UpdateFundGoalForm fundGoal={fundGoal} redirectUrl={redirectUrl} />
      }
    >
      <ResponsiveGrid columns={{ xs: 1 }} spacing={2}>
        <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
          <ReadOnlyField label="Fund" value={fundGoal.fund.name} />
          <ReadOnlyField
            label="Accounting Period"
            value={fundGoal.accountingPeriod?.name ?? "Onboarded"}
          />
          <ReadOnlyField
            label="Minimum Ending Balance"
            value={formatCurrency(fundGoal.minimumEndingBalance)}
          />
          <ReadOnlyField
            label="Maximum Ending Balance"
            value={
              isNullOrUndefined(fundGoal.maximumEndingBalance)
                ? "Not configured"
                : formatCurrency(fundGoal.maximumEndingBalance)
            }
          />
          <ReadOnlyField
            label="Allow expected contribution to exceed maximum"
            value={
              isNullOrUndefined(fundGoal.maximumEndingBalance)
                ? "Not applicable"
                : fundGoal.allowExpectedContributionAboveMaximum === true
                  ? "Yes"
                  : "No"
            }
          />
          <ReadOnlyField
            label="Planned Monthly Contribution"
            value={
              isNullOrUndefined(fundGoal.plannedMonthlyContribution)
                ? "Not configured"
                : formatCurrency(fundGoal.plannedMonthlyContribution)
            }
          />
        </ResponsiveGrid>
      </ResponsiveGrid>
    </Frame>
  );
};
export default FundGoalContextFrame;
