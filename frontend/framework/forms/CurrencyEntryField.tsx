import { InputAdornment, TextField } from "@mui/material";
import { type JSX, useEffect, useState } from "react";
import {
  currencyEditPattern,
  formatCurrencyValue,
  parseCurrencyValue,
  sanitizeCurrencyInput,
} from "@/framework/currencyHelpers";

/**
 * Props for the CurrencyEntryField component.
 */
interface CurrencyEntryFieldProps {
  readonly label: string;
  readonly value: number | null;
  readonly setValue?: ((newValue: number | null) => void) | null;
  readonly errorMessage?: string | null;
  readonly disabled?: boolean;
}

/**
 * Component the presents the user with an entry field where they can enter currency values.
 */
const CurrencyEntryField = function ({
  label,
  value,
  setValue = null,
  errorMessage = null,
  disabled = false,
}: CurrencyEntryFieldProps): JSX.Element {
  const [isEditing, setIsEditing] = useState<boolean>(false);
  const [stringValue, setStringValue] = useState<string>(
    value === null ? "" : formatCurrencyValue(value),
  );

  useEffect(() => {
    if (isEditing) {
      return;
    }

    setStringValue(value === null ? "" : formatCurrencyValue(value));
  }, [isEditing, value]);

  return (
    <TextField
      className="currency-entry-field"
      label={label}
      variant="outlined"
      value={stringValue}
      disabled={disabled}
      slotProps={{
        input: {
          readOnly: setValue === null,
          startAdornment: <InputAdornment position="start">$</InputAdornment>,
        },
      }}
      onFocus={() => {
        setIsEditing(true);
        setStringValue((currentValue) => sanitizeCurrencyInput(currentValue));
      }}
      onChange={(event) => {
        const nextValue = sanitizeCurrencyInput(event.target.value);
        if (!currencyEditPattern.test(nextValue)) {
          return;
        }

        setStringValue(nextValue);
        setValue?.(parseCurrencyValue(nextValue));
      }}
      onBlur={() => {
        setIsEditing(false);

        const parsedValue = parseCurrencyValue(stringValue);
        if (parsedValue === null) {
          setStringValue("");
          setValue?.(null);
          return;
        }

        setStringValue(formatCurrencyValue(parsedValue));
        setValue?.(parsedValue);
      }}
      error={errorMessage !== null}
      helperText={errorMessage ?? null}
    />
  );
};

export default CurrencyEntryField;
