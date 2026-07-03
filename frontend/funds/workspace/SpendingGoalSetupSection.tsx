import { Box, Stack, Typography } from "@mui/material";
import {
  SpendingGoalType,
  describeSpendingGoalType,
  formatSpendingGoalType,
} from "@/goals/types";
import Frame from "@/framework/view/Frame";
import FundGoalTypeEntryField from "@/funds/FundGoalTypeEntryField";
import type { JSX } from "react";

interface SpendingGoalSetupSectionProps {
  readonly value: SpendingGoalType | null;
  readonly setValue: ((newValue: SpendingGoalType | null) => void) | null;
  readonly typeErrorMessage?: string | null;
}

/**
 * Renders the shared spending goal setup section used by fund and goal forms.
 */
const SpendingGoalSetupSection = function ({
  value,
  setValue,
  typeErrorMessage = null,
}: SpendingGoalSetupSectionProps): JSX.Element {
  return (
    <Frame title="Spending Goal Setup">
      <Stack spacing={2}>
        <FundGoalTypeEntryField<SpendingGoalType>
          label="Spending Goal Type"
          options={[SpendingGoalType.Standard, SpendingGoalType.Debt]}
          value={value}
          setValue={setValue}
          formatOptionLabel={formatSpendingGoalType}
          errorMessage={typeErrorMessage}
        />

        <Box
          sx={{
            px: 2,
            py: 1.5,
            borderRadius: 3,
            border: "1px solid",
            borderColor: "divider",
            backgroundColor: "action.hover",
          }}
        >
          <Stack spacing={0.5}>
            <Typography variant="subtitle2">
              {value === null
                ? "Choose a spending goal type"
                : formatSpendingGoalType(value)}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {value === null
                ? "Select an option to see how spending progress will be evaluated."
                : describeSpendingGoalType(value)}
            </Typography>
          </Stack>
        </Box>
      </Stack>
    </Frame>
  );
};

export default SpendingGoalSetupSection;
