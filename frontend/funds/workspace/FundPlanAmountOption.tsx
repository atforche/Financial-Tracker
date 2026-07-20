import { Checkbox, FormControlLabel, Stack, Typography } from "@mui/material";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";

/**
 * Props for a configurable Fund Plan amount.
 */
interface FundPlanAmountOptionProps {
  readonly label: string;
  readonly description: string;
  readonly value: number | null;
  readonly setValue: ((value: number | null) => void) | null;
  readonly errorMessage: string | null;
}

/**
 * Renders an optional Fund Plan amount with its enablement control.
 */
const FundPlanAmountOption = function ({
  label,
  description,
  value,
  setValue,
  errorMessage,
}: FundPlanAmountOptionProps): JSX.Element {
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
      <Typography variant="body2" color="text.secondary">
        {description}
      </Typography>
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

export default FundPlanAmountOption;
