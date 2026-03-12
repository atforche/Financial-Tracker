import type { JSX } from "react";
import { TextField } from "@mui/material";

/**
 * Props for the IntegerEntryField component.
 */
interface IntegerEntryFieldProps {
  readonly name: string;
  readonly label: string;
  readonly defaultValue: number | null;
  readonly errorMessage?: string | null;
}

/**
 * Component the presents the user with an entry field where they can enter integer values.
 */
const IntegerEntryField = function ({
  name,
  label,
  defaultValue,
  errorMessage,
}: IntegerEntryFieldProps): JSX.Element {
  return (
    <TextField
      name={name}
      label={label}
      variant="outlined"
      defaultValue={defaultValue}
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
    />
  );
};

export default IntegerEntryField;
