import Frame, { type FrameColor } from "@/framework/view/Frame";
import FundPlanAmountOption from "@/funds/workspace/FundPlanAmountOption";
import type { JSX } from "react";
import { Stack } from "@mui/material";

/**
 * Props for the FundPlanSetupSection component.
 */
interface FundPlanSetupSectionProps {
  readonly color?: FrameColor;
  readonly regularContribution: number | null;
  readonly setRegularContribution: ((value: number | null) => void) | null;
  readonly minimumFundedBalance: number | null;
  readonly setMinimumFundedBalance: ((value: number | null) => void) | null;
  readonly maximumFundedBalance: number | null;
  readonly setMaximumFundedBalance: ((value: number | null) => void) | null;
  readonly targetEndingBalance: number | null;
  readonly setTargetEndingBalance: ((value: number | null) => void) | null;
  readonly errors?: Partial<
    Record<
      | "regularContribution"
      | "minimumFundedBalance"
      | "maximumFundedBalance"
      | "targetEndingBalance",
      string | null
    >
  >;
}

/**
 * Renders the configurable quantities in a Funding Plan.
 */
const FundPlanSetupSection = function ({
  color = "primary",
  regularContribution,
  setRegularContribution,
  minimumFundedBalance,
  setMinimumFundedBalance,
  maximumFundedBalance,
  setMaximumFundedBalance,
  targetEndingBalance,
  setTargetEndingBalance,
  errors,
}: FundPlanSetupSectionProps): JSX.Element {
  return (
    <Frame title="Funding Plan Setup" color={color}>
      <Stack spacing={2}>
        <FundPlanAmountOption
          label="Regular Monthly Contribution"
          description="The amount you normally plan to add during each accounting period."
          value={regularContribution}
          setValue={setRegularContribution}
          errorMessage={errors?.regularContribution ?? null}
        />
        <FundPlanAmountOption
          label="Minimum Funded Amount"
          description="The lowest balance you want the fund to have immediately after assignments."
          value={minimumFundedBalance}
          setValue={setMinimumFundedBalance}
          errorMessage={errors?.minimumFundedBalance ?? null}
        />
        <FundPlanAmountOption
          label="Maximum Funded Amount"
          description="The highest balance you want the fund to have immediately after assignments."
          value={maximumFundedBalance}
          setValue={setMaximumFundedBalance}
          errorMessage={errors?.maximumFundedBalance ?? null}
        />
        <FundPlanAmountOption
          label="Target Ending Balance"
          description="The balance you want remaining at the end of the accounting period."
          value={targetEndingBalance}
          setValue={setTargetEndingBalance}
          errorMessage={errors?.targetEndingBalance ?? null}
        />
      </Stack>
    </Frame>
  );
};

export default FundPlanSetupSection;
