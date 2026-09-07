import { Checkbox, FormControlLabel, Stack } from "@mui/material";
import type { JSX, ReactNode } from "react";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";

/**
 * Props for an optional goal amount.
 */
interface GoalAmountOptionProps {
  readonly label: string;
  readonly value: number | null;
  readonly setValue: ((value: number | null) => void) | null;
  readonly errorMessage: string | null;
  readonly additionalControl?: ReactNode;
}

/**
 * Renders an optional goal amount with its enablement control.
 */
const GoalAmountOption = function ({
  label,
  value,
  setValue,
  errorMessage,
  additionalControl,
}: GoalAmountOptionProps): JSX.Element {
  const enabled = value !== null;
  const amountField = (
    <CurrencyEntryField
      label="Amount"
      value={value}
      setValue={setValue}
      errorMessage={errorMessage}
      disabled={!enabled}
      {...(additionalControl === undefined ? {} : { sx: { width: "100%" } })}
    />
  );

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
      {additionalControl === undefined ? (
        amountField
      ) : (
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={1}
          useFlexGap
          alignItems="center"
          sx={{
            "& > *": {
              flex: {
                xs: "1 1 100%",
                sm: "1 1 0",
              },
              minWidth: 0,
              width: "100%",
            },
          }}
        >
          {amountField}
          {additionalControl}
        </Stack>
      )}
    </Stack>
  );
};

export default GoalAmountOption;
