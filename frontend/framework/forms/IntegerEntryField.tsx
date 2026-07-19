import { type JSX, useEffect, useState } from "react";
import { TextField } from "@mui/material";

/**
 * Props for the IntegerEntryField component.
 */
interface IntegerEntryFieldProps {
  readonly label: string;
  readonly value: number | null;
  readonly setValue?: ((value: number | null) => void) | null;
  readonly errorMessage?: string | null;
  readonly disabled?: boolean;
}

/**
 * Component the presents the user with an entry field where they can enter integer values.
 */
const IntegerEntryField = function ({
  label,
  value,
  setValue = null,
  errorMessage = null,
  disabled = false,
}: IntegerEntryFieldProps): JSX.Element {
  const [stringValue, setStringValue] = useState(value?.toString() ?? "");

  useEffect(() => {
    setStringValue(value?.toString() ?? "");
  }, [value]);

  return (
    <TextField
      label={label}
      variant="outlined"
      value={stringValue}
      disabled={disabled}
      error={errorMessage !== null}
      helperText={errorMessage}
      slotProps={{
        input: {
          inputMode: "numeric",
          readOnly: setValue === null,
        },
        htmlInput: {
          pattern: "[0-9]*",
        },
      }}
      onChange={(event) => {
        const nextValue = event.target.value;
        if (!/^\d*$/u.test(nextValue)) {
          return;
        }

        setStringValue(nextValue);
        setValue?.(nextValue === "" ? null : Number(nextValue));
      }}
    />
  );
};

export default IntegerEntryField;
