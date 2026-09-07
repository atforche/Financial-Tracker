import { type JSX, useEffect, useState } from "react";
import ReadOnlyField from "@/framework/forms/ReadOnlyField";
import { TextField } from "@mui/material";

/**
 * Props for the IntegerEntryField component.
 */
interface IntegerEntryFieldProps {
  readonly value: number | null;
  readonly setValue?: ((value: number | null) => void) | null;
  readonly errorMessage?: string | null;
}

/**
 * Component the presents the user with an entry field where they can enter integer values.
 */
const IntegerEntryField = function ({
  value,
  setValue = null,
  errorMessage = null,
}: IntegerEntryFieldProps): JSX.Element {
  const [stringValue, setStringValue] = useState(value?.toString() ?? "");

  useEffect(() => {
    setStringValue(value?.toString() ?? "");
  }, [value]);

  if (setValue === null) {
    return (
      <ReadOnlyField
        label="Year"
        value={value === null ? null : value.toLocaleString()}
      />
    );
  }

  return (
    <TextField
      label="Year"
      variant="outlined"
      value={stringValue}
      error={errorMessage !== null}
      helperText={errorMessage}
      slotProps={{
        input: {
          inputMode: "numeric",
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
        setValue(nextValue === "" ? null : Number(nextValue));
      }}
    />
  );
};

export default IntegerEntryField;
