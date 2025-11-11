import type { JSX } from "react";
import { TextField } from "@mui/material";

/**
 * Props for the StringEntryField component.
 * @param {string} label - Label for this String Entry Field.
 * @param {Function} setValue - Callback to update the value in this String Entry Field. If null, this field is read-only.
 * @param {string} value - Current value for this String Entry Field.
 */
interface StringEntryFieldProps {
  label: string;
  value: string;
  setValue?: ((newValue: string) => void) | null;
}

/**
 * Component the presents the user with an entry field where they can enter string values.
 * @param {StringEntryFieldProps} props - Props for the StringEntryField component.
 * @returns {JSX.Element} JSX element representing the StringEntryField component.
 */
const StringEntryField = function ({
  label,
  value,
  setValue = null,
}: StringEntryFieldProps): JSX.Element {
  return (
    <TextField
      label={label}
      variant="outlined"
      value={value}
      slotProps={{
        input: {
          readOnly: setValue === null,
        },
      }}
      onChange={(event) => {
        setValue?.(event.target.value);
      }}
    />
  );
};

export default StringEntryField;
