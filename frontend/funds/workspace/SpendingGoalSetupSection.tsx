import Frame, { type FrameColor } from "@/framework/view/Frame";
import {
  describeSpendingGoalType,
  formatSpendingGoalType,
} from "@/goals/helpers";
import FundGoalTypeEntryField from "@/funds/FundGoalTypeEntryField";
import GoalTypeDescription from "@/funds/workspace/GoalTypeDescription";
import type { JSX } from "react";
import { SpendingGoalType } from "@/goals/types";
import { Stack } from "@mui/material";

/**
 * Props for the SpendingGoalSetupSection component.
 */
interface SpendingGoalSetupSectionProps {
  readonly color?: FrameColor;
  readonly value: SpendingGoalType | null;
  readonly setValue: ((newValue: SpendingGoalType | null) => void) | null;
  readonly typeErrorMessage?: string | null;
}

/**
 * Renders the shared spending goal setup section used by fund and goal forms.
 */
const SpendingGoalSetupSection = function ({
  color = "primary",
  value,
  setValue,
  typeErrorMessage = null,
}: SpendingGoalSetupSectionProps): JSX.Element {
  return (
    <Frame title="Spending Goal Setup" color={color}>
      <Stack spacing={2}>
        <FundGoalTypeEntryField<SpendingGoalType>
          label="Spending Goal Type"
          options={[SpendingGoalType.Standard, SpendingGoalType.Debt]}
          value={value}
          setValue={setValue}
          formatOptionLabel={formatSpendingGoalType}
          errorMessage={typeErrorMessage}
        />

        <GoalTypeDescription
          title={
            value === null
              ? "Choose a spending goal type"
              : formatSpendingGoalType(value)
          }
          description={
            value === null
              ? "Select an option to see how spending progress will be evaluated."
              : describeSpendingGoalType(value)
          }
        />
      </Stack>
    </Frame>
  );
};

export default SpendingGoalSetupSection;
