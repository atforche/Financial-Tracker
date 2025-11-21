import { type JSX, useCallback } from "react";
import type { ApiErrorDetail } from "@data/ApiError";
import { TextField } from "@mui/material";

/**
 * Props for the StringEntryField component.
 * @param {string} label - Label for this String Entry Field.
 * @param {(newValue: string) => void | null} setValue - Callback to update the value in this String Entry Field. If null, this field is read-only.
 * @param {string} value - Current value for this String Entry Field.
 */
interface StringEntryFieldProps {
  readonly label: string;
  readonly value: string;
  readonly setValue?: ((newValue: string) => void) | null;
  readonly error?: ApiErrorDetail | null;
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
  error = null,
}: StringEntryFieldProps): JSX.Element {
  const onChange = useCallback(
    (event: React.ChangeEvent<HTMLInputElement>) => {
      setValue?.(event.target.value);
    },
    [setValue],
  );

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
      onChange={onChange}
      error={error !== null}
      helperText={error?.description ?? ""}
    />
  );
};

export default StringEntryField;
