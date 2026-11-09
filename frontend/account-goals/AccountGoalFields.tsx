import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import GoalAmountOption from "@/framework/forms/GoalAmountOption";
import type { JSX } from "react";
import { Stack } from "@mui/material";

/**
 * Props for the AccountGoalFields component.
 */
interface AccountGoalFieldsProps {
  readonly minimumEndingBalance: number;
  readonly setMinimumEndingBalance: (value: number) => void;
  readonly maximumEndingBalance: number | null;
  readonly setMaximumEndingBalance: (value: number | null) => void;
  readonly minimumEndingBalanceErrors: string | null;
  readonly maximumEndingBalanceErrors: string | null;
}

/**
 * Entry fields for an account goal's minimum and optional maximum ending balance.
 */
const AccountGoalFields = function ({
  minimumEndingBalance,
  setMinimumEndingBalance,
  maximumEndingBalance,
  setMaximumEndingBalance,
  minimumEndingBalanceErrors,
  maximumEndingBalanceErrors,
}: AccountGoalFieldsProps): JSX.Element {
  return (
    <Stack spacing={2}>
      <CurrencyEntryField
        label="Minimum Ending Balance"
        value={minimumEndingBalance}
        setValue={(value) => {
          setMinimumEndingBalance(value ?? 0);
        }}
        errorMessage={minimumEndingBalanceErrors}
      />
      <GoalAmountOption
        label="Maximum Ending Balance"
        value={maximumEndingBalance}
        setValue={setMaximumEndingBalance}
        errorMessage={maximumEndingBalanceErrors}
      />
    </Stack>
  );
};

export default AccountGoalFields;
