import type {
  FundGoal,
  FundGoalProgress as FundGoalProgressModel,
} from "@/fund-goals/types";
import Frame from "@/framework/view/Frame";
import FundGoalProgressBars from "@/fund-goals/workspace/FundGoalProgressBars";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";
import UpdateFundGoalForm from "@/fund-goals/workspace/UpdateFundGoalForm";

/**
 * Props for the FundGoalContextFrame component.
 */
interface FundGoalContextFrameProps {
  readonly fundGoal: FundGoal;
  readonly progress: FundGoalProgressModel;
  readonly redirectUrl: string;
}

/**
 * Displays a Fund Goal's context and update action.
 */
const FundGoalContextFrame = function ({
  fundGoal,
  progress,
  redirectUrl,
}: FundGoalContextFrameProps): JSX.Element {
  return (
    <Frame
      title="Fund Goal"
      headerContent={
        <UpdateFundGoalForm fundGoal={fundGoal} redirectUrl={redirectUrl} />
      }
    >
      <ResponsiveGrid columns={{ xs: 1 }} spacing={2}>
        <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
          <StringEntryField
            label="Accounting Period"
            value={fundGoal.accountingPeriod?.name ?? "Onboarded"}
            setValue={null}
          />
          <StringEntryField
            label="Fund"
            value={fundGoal.fund.name}
            setValue={null}
          />
        </ResponsiveGrid>
        <FundGoalProgressBars
          fundGoal={fundGoal}
          progress={progress}
          showUnconfigured
        />
      </ResponsiveGrid>
    </Frame>
  );
};
export default FundGoalContextFrame;
