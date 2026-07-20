import Frame from "@/framework/view/Frame";
import type { FundPlan } from "@/fund-plans/types";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";
import UpdateFundPlanForm from "@/fund-plans/workspace/UpdateFundPlanForm";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the FundPlanContextFrame component.
 */
interface FundPlanContextFrameProps {
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
 * Displays a Funding Plan's context and update action.
 */
const FundPlanContextFrame = function ({
  fundPlan,
  redirectUrl,
}: FundPlanContextFrameProps): JSX.Element {
  return (
    <Frame
      title="Funding Plan"
      headerContent={
        <UpdateFundPlanForm fundPlan={fundPlan} redirectUrl={redirectUrl} />
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
export default FundPlanContextFrame;
