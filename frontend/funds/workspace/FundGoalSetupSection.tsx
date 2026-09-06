import Frame, { type FrameColor } from "@/framework/view/Frame";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import FundGoalAmountOption from "@/funds/workspace/FundGoalAmountOption";
import type { JSX } from "react";
import { Stack } from "@mui/material";

/**
 * Props for the FundGoalSetupSection component.
 */
interface FundGoalSetupSectionProps {
  readonly color?: FrameColor;
  readonly showFrame?: boolean;
  readonly plannedMonthlyContribution: number | null;
  readonly setPlannedMonthlyContribution:
    ((value: number | null) => void) | null;
  readonly minimumEndingBalance: number;
  readonly setMinimumEndingBalance: ((value: number) => void) | null;
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
  showFrame = true,
  plannedMonthlyContribution,
  setPlannedMonthlyContribution,
  minimumEndingBalance,
  setMinimumEndingBalance,
  maximumEndingBalance,
  setMaximumEndingBalance,
  errors,
}: FundGoalSetupSectionProps): JSX.Element {
  const content = (
    <Stack spacing={2}>
      <CurrencyEntryField
        label="Minimum Ending Balance"
        value={minimumEndingBalance}
        setValue={
          setMinimumEndingBalance === null
            ? null
            : (value): void => {
                setMinimumEndingBalance(value ?? 0);
              }
        }
        errorMessage={errors?.minimumEndingBalance ?? null}
      />
      <FundGoalAmountOption
        label="Planned Monthly Contribution"
        value={plannedMonthlyContribution}
        setValue={setPlannedMonthlyContribution}
        errorMessage={errors?.plannedMonthlyContribution ?? null}
      />
      <FundGoalAmountOption
        label="Maximum Ending Balance"
        value={maximumEndingBalance}
        setValue={setMaximumEndingBalance}
        errorMessage={errors?.maximumEndingBalance ?? null}
      />
    </Stack>
  );
  return showFrame ? (
    <Frame title="Fund Goal Setup" color={color}>
      {content}
    </Frame>
  ) : (
    content
  );
};

export default FundGoalSetupSection;
