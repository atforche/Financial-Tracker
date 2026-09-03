import type { JSX } from "react";
import ReadOnlyField from "@/framework/forms/ReadOnlyField";
import { TextField } from "@mui/material";

/**
 * Props for the StringEntryField component.
 */
interface StringEntryFieldProps {
  readonly label: string;
  readonly value: string | null;
  readonly setValue?: ((newValue: string) => void) | null;
  readonly errorMessage?: string | null;
  readonly disabled?: boolean;
  readonly autoFocus?: boolean;
}

/**
 * Component the presents the user with an entry field where they can enter string values.
 */
const StringEntryField = function ({
  label,
  value,
  setValue = null,
  errorMessage = null,
  disabled = false,
  autoFocus = false,
}: StringEntryFieldProps): JSX.Element {
  if (setValue === null && !disabled) {
    return <ReadOnlyField label={label} value={value} />;
  }

  return (
    <TextField
      label={label}
      variant="outlined"
      value={value ?? ""}
      disabled={disabled}
      autoFocus={autoFocus}
      slotProps={{
        input: {
          readOnly: setValue === null,
        },
      }}
      onChange={(event) => {
        setValue?.(event.target.value);
      }}
      error={errorMessage !== null}
      helperText={errorMessage ?? null}
      sx={{ width: "100%" }}
    />
  );
};

export default StringEntryField;
