import {
  AssignmentGoalType,
  describeAssignmentGoalType,
  formatAssignmentGoalType,
} from "@/goals/types";
import { Box, Stack, Typography } from "@mui/material";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import FundGoalTypeEntryField from "@/funds/FundGoalTypeEntryField";
import type { JSX } from "react";
import TransactionSection from "@/transactions/workspace/TransactionSection";

interface AssignmentGoalSetupSectionProps {
  readonly value: AssignmentGoalType | null;
  readonly setValue: ((newValue: AssignmentGoalType | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((newValue: number | null) => void) | null;
  readonly typeErrorMessage?: string | null;
  readonly amountErrorMessage?: string | null;
}

const getAssignmentAmountHelperText = function (
  goalType: AssignmentGoalType | null,
): string {
  if (goalType === null) {
    return "Choose the assignment behavior that matches how you want to fund this category.";
  }
  switch (goalType) {
    case AssignmentGoalType.MonthlyTarget:
      return "Enter the ending balance you want this fund to reach for the period.";
    case AssignmentGoalType.RecurringContribution:
      return "Enter the amount you want to assign into this fund during the period.";
    default:
      return goalType satisfies never;
  }
};

/**
 * Renders the shared assignment goal setup section used by fund and goal forms.
 */
const AssignmentGoalSetupSection = function ({
  value,
  setValue,
  amount,
  setAmount,
  typeErrorMessage = null,
  amountErrorMessage = null,
}: AssignmentGoalSetupSectionProps): JSX.Element {
  return (
    <TransactionSection
      title="Assignment Goal Setup"
      description="Choose how this fund should be funded."
    >
      <Stack spacing={2}>
        <FundGoalTypeEntryField<AssignmentGoalType>
          label="Assignment Goal Type"
          options={[
            AssignmentGoalType.MonthlyTarget,
            AssignmentGoalType.RecurringContribution,
          ]}
          value={value}
          setValue={setValue}
          formatOptionLabel={formatAssignmentGoalType}
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
                ? "Choose an assignment goal type"
                : formatAssignmentGoalType(value)}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {value === null
                ? "Select an option to see how the assignment amount will be interpreted."
                : describeAssignmentGoalType(value)}
            </Typography>
          </Stack>
        </Box>

        <Stack spacing={0.75}>
          <CurrencyEntryField
            label="Assignment Goal Amount"
            value={amount}
            setValue={setAmount}
            errorMessage={amountErrorMessage}
          />
          <Typography variant="body2" color="text.secondary">
            {getAssignmentAmountHelperText(value)}
          </Typography>
        </Stack>
      </Stack>
    </TransactionSection>
  );
};

export default AssignmentGoalSetupSection;
