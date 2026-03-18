import type { JSX } from "react";
import { TextField } from "@mui/material";

/**
 * Props for the StringEntryField component.
 */
interface StringEntryFieldProps {
  readonly label: string;
  readonly value: string;
  readonly setValue: (newValue: string) => void;
  readonly errorMessage?: string | null;
}

/**
 * Component the presents the user with an entry field where they can enter string values.
 */
const StringEntryField = function ({
  label,
  value,
  setValue,
  errorMessage = null,
}: StringEntryFieldProps): JSX.Element {
  return (
    <TextField
      label={label}
      variant="outlined"
      value={value}
      onChange={(event) => {
        setValue(event.target.value);
      }}
      error={errorMessage !== null}
      helperText={errorMessage ?? null}
    />
  );
};

export default StringEntryField;
