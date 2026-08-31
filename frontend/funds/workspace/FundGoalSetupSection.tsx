import Frame, { type FrameColor } from "@/framework/view/Frame";
import { type JSX, useRef } from "react";
import FundGoalAmountOption from "@/funds/workspace/FundGoalAmountOption";
import { Stack } from "@mui/material";

/**
 * Props for the FundGoalSetupSection component.
 */
interface FundGoalSetupSectionProps {
  readonly color?: FrameColor;
  readonly plannedMonthlyContribution: number | null;
  readonly setPlannedMonthlyContribution:
    ((value: number | null) => void) | null;
  readonly minimumEndingBalance: number | null;
  readonly setMinimumEndingBalance: ((value: number | null) => void) | null;
  readonly maximumEndingBalance: number | null;
  readonly setMaximumEndingBalance: ((value: number | null) => void) | null;
  readonly errors?: Partial<
    Record<
      | "plannedMonthlyContribution"
      | "minimumEndingBalance"
      | "maximumEndingBalance",
      string | null
    >
  >;
}

/**
 * Renders the configurable quantities in a Fund Goal.
 */
const FundGoalSetupSection = function ({
  color = "primary",
  plannedMonthlyContribution,
  setPlannedMonthlyContribution,
  minimumEndingBalance,
  setMinimumEndingBalance,
  maximumEndingBalance,
  setMaximumEndingBalance,
  errors,
}: FundGoalSetupSectionProps): JSX.Element {
  const autoFilledBalance = useRef<"minimum" | "maximum" | null>(null);
  const setMinimumEndingBalanceWithDefault =
    setMinimumEndingBalance === null
      ? null
      : (value: number | null): void => {
          setMinimumEndingBalance(value);
          if (autoFilledBalance.current === "minimum") {
            autoFilledBalance.current = null;
          } else if (value === null) {
            autoFilledBalance.current = null;
          } else if (maximumEndingBalance === null) {
            autoFilledBalance.current = "maximum";
            setMaximumEndingBalance?.(value);
          } else if (autoFilledBalance.current === "maximum") {
            setMaximumEndingBalance?.(value);
          }
        };
  const setMaximumEndingBalanceWithDefault =
    setMaximumEndingBalance === null
      ? null
      : (value: number | null): void => {
          setMaximumEndingBalance(value);
          if (autoFilledBalance.current === "maximum") {
            autoFilledBalance.current = null;
          } else if (value === null) {
            autoFilledBalance.current = null;
          } else if (minimumEndingBalance === null) {
            autoFilledBalance.current = "minimum";
            setMinimumEndingBalance?.(value);
          } else if (autoFilledBalance.current === "minimum") {
            setMinimumEndingBalance?.(value);
          }
        };

  return (
    <Frame title="Fund Goal Setup" color={color}>
      <Stack spacing={2}>
        <FundGoalAmountOption
          label="Planned Monthly Contribution"
          description="This is a baseline amount that should be contributed to the fund every accounting period."
          value={plannedMonthlyContribution}
          setValue={setPlannedMonthlyContribution}
          errorMessage={errors?.plannedMonthlyContribution ?? null}
        />
        <FundGoalAmountOption
          label="Minimum Ending Balance"
          description="This is the minimum balance you want remaining at the end of the accounting period."
          value={minimumEndingBalance}
          setValue={setMinimumEndingBalanceWithDefault}
          errorMessage={errors?.minimumEndingBalance ?? null}
        />
        <FundGoalAmountOption
          label="Maximum Ending Balance"
          description="This is the maximum balance you want remaining at the end of the accounting period. It also caps the expected contribution."
          value={maximumEndingBalance}
          setValue={setMaximumEndingBalanceWithDefault}
          errorMessage={errors?.maximumEndingBalance ?? null}
        />
      </Stack>
    </Frame>
  );
};

export default FundGoalSetupSection;
