import { InputAdornment, TextField } from "@mui/material";
import { type JSX, useEffect, useState } from "react";
import type { SxProps, Theme } from "@mui/material/styles";
import {
  currencyEditPattern,
  formatCurrency,
  formatCurrencyValue,
  parseCurrencyValue,
  sanitizeCurrencyInput,
} from "@/framework/currencyHelpers";
import ReadOnlyField from "@/framework/forms/ReadOnlyField";

/**
 * Props for the CurrencyEntryField component.
 */
interface CurrencyEntryFieldProps {
  readonly label: string;
  readonly value: number | null;
  readonly setValue?: ((newValue: number | null) => void) | null;
  readonly errorMessage?: string | null;
  readonly disabled?: boolean;
  readonly autoFocus?: boolean;
  readonly sx?: Exclude<SxProps<Theme>, readonly unknown[]>;
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
  autoFocus = false,
  sx,
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

  if (setValue === null && !disabled) {
    return (
      <ReadOnlyField
        label={label}
        value={value === null ? null : formatCurrency(value)}
        {...(sx === undefined ? {} : { sx })}
      />
    );
  }

  return (
    <TextField
      className="currency-entry-field"
      label={label}
      variant="outlined"
      value={stringValue}
      disabled={disabled}
      autoFocus={autoFocus}
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
        const parsedValue = parseCurrencyValue(nextValue);
        if (parsedValue !== null) {
          setValue?.(parsedValue);
        }
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
      sx={sx}
    />
  );
};

export default CurrencyEntryField;
