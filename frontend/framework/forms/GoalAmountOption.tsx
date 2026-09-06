import { Checkbox, FormControlLabel, Stack } from "@mui/material";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";

/**
 * Props for an optional goal amount.
 */
interface GoalAmountOptionProps {
  readonly label: string;
  readonly value: number | null;
  readonly setValue: ((value: number | null) => void) | null;
  readonly errorMessage: string | null;
}

/**
 * Renders an optional goal amount with its enablement control.
 */
const GoalAmountOption = function ({
  label,
  value,
  setValue,
  errorMessage,
}: GoalAmountOptionProps): JSX.Element {
  const enabled = value !== null;

  return (
    <Stack spacing={1}>
      <FormControlLabel
        control={
          <Checkbox
            checked={enabled}
            disabled={setValue === null}
            onChange={(event) => {
              setValue?.(event.target.checked ? 0 : null);
            }}
          />
        }
        label={label}
      />
      <CurrencyEntryField
        label="Amount"
        value={value}
        setValue={setValue}
        errorMessage={errorMessage}
        disabled={!enabled}
      />
    </Stack>
  );
};

export default GoalAmountOption;
