import Frame, { type FrameColor } from "@/framework/view/Frame";
import { type JSX, useRef } from "react";
import FundGoalAmountOption from "@/funds/workspace/FundGoalAmountOption";
import { Stack } from "@mui/material";

/**
 * Props for the FundGoalSetupSection component.
 */
interface FundGoalSetupSectionProps {
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
 * Renders the configurable quantities in a Fund Goal.
 */
const FundGoalSetupSection = function ({
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
}: FundGoalSetupSectionProps): JSX.Element {
  const autoFilledBalance = useRef<"minimum" | "maximum" | null>(null);
  const setMinimumFundedBalanceWithDefault =
    setMinimumFundedBalance === null
      ? null
      : (value: number | null): void => {
          setMinimumFundedBalance(value);
          if (autoFilledBalance.current === "minimum") {
            autoFilledBalance.current = null;
          } else if (value === null) {
            autoFilledBalance.current = null;
          } else if (maximumFundedBalance === null) {
            autoFilledBalance.current = "maximum";
            setMaximumFundedBalance?.(value);
          } else if (autoFilledBalance.current === "maximum") {
            setMaximumFundedBalance?.(value);
          }
        };
  const setMaximumFundedBalanceWithDefault =
    setMaximumFundedBalance === null
      ? null
      : (value: number | null): void => {
          setMaximumFundedBalance(value);
          if (autoFilledBalance.current === "maximum") {
            autoFilledBalance.current = null;
          } else if (value === null) {
            autoFilledBalance.current = null;
          } else if (minimumFundedBalance === null) {
            autoFilledBalance.current = "minimum";
            setMinimumFundedBalance?.(value);
          } else if (autoFilledBalance.current === "minimum") {
            setMinimumFundedBalance?.(value);
          }
        };

  return (
    <Frame title="Fund Goal Setup" color={color}>
      <Stack spacing={2}>
        <FundGoalAmountOption
          label="Regular Monthly Contribution"
          description="This is a baseline amount that should be contributed to the fund every accounting period."
          value={regularContribution}
          setValue={setRegularContribution}
          errorMessage={errors?.regularContribution ?? null}
        />
        <FundGoalAmountOption
          label="Minimum Funded Amount"
          description="This is the minimum amount that should always be available to spend from this fund every month."
          value={minimumFundedBalance}
          setValue={setMinimumFundedBalanceWithDefault}
          errorMessage={errors?.minimumFundedBalance ?? null}
        />
        <FundGoalAmountOption
          label="Maximum Funded Amount"
          description="This is the maximum amount that should be available to spend from this fund."
          value={maximumFundedBalance}
          setValue={setMaximumFundedBalanceWithDefault}
          errorMessage={errors?.maximumFundedBalance ?? null}
        />
        <FundGoalAmountOption
          label="Target Ending Balance"
          description="The target balance you want remaining at the end of the accounting period."
          value={targetEndingBalance}
          setValue={setTargetEndingBalance}
          errorMessage={errors?.targetEndingBalance ?? null}
        />
      </Stack>
    </Frame>
  );
};

export default FundGoalSetupSection;
