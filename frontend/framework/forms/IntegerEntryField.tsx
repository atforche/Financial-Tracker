import type { JSX } from "react";
import { TextField } from "@mui/material";

/**
 * Props for the IntegerEntryField component.
 */
interface IntegerEntryFieldProps {
  readonly label: string;
  readonly value: number | null;
  readonly setValue: (value: number | null) => void;
  readonly errorMessage?: string | null;
}

/**
 * Component the presents the user with an entry field where they can enter integer values.
 */
const IntegerEntryField = function ({
  label,
  value,
  setValue,
  errorMessage,
}: IntegerEntryFieldProps): JSX.Element {
  return (
    <TextField
      label={label}
      variant="outlined"
      value={value ?? ""}
      error={(errorMessage ?? null) !== null}
      helperText={errorMessage}
      slotProps={{
        input: {
          inputMode: "numeric",
        },
        htmlInput: {
          pattern: "[0-9]*",
        },
      }}
      onChange={(e) => {
        const newValue =
          e.target.value === "" ? null : parseInt(e.target.value, 10);
        setValue(newValue);
      }}
    />
  );
};

export default IntegerEntryField;
