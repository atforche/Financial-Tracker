import type {
  FundPlan,
  FundPlanProgress as FundPlanProgressModel,
} from "@/fund-plans/types";
import Frame from "@/framework/view/Frame";
import FundPlanProgressBars from "@/fund-plans/workspace/FundPlanProgressBars";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";
import UpdateFundPlanForm from "@/fund-plans/workspace/UpdateFundPlanForm";

/**
 * Props for the FundPlanContextFrame component.
 */
interface FundPlanContextFrameProps {
  readonly fundPlan: FundPlan;
  readonly progress: FundPlanProgressModel;
  readonly redirectUrl: string;
}

/**
 * Displays a Funding Plan's context and update action.
 */
const FundPlanContextFrame = function ({
  fundPlan,
  progress,
  redirectUrl,
}: FundPlanContextFrameProps): JSX.Element {
  return (
    <Frame
      title="Funding Plan"
      headerContent={
        <UpdateFundPlanForm fundPlan={fundPlan} redirectUrl={redirectUrl} />
      }
    >
      <ResponsiveGrid columns={{ xs: 1 }} spacing={2}>
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
        </ResponsiveGrid>
        <FundPlanProgressBars
          fundPlan={fundPlan}
          progress={progress}
          showUnconfigured
        />
      </ResponsiveGrid>
    </Frame>
  );
};
export default FundPlanContextFrame;
