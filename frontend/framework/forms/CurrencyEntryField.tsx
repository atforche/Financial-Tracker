import { InputAdornment, TextField } from "@mui/material";
import { type JSX, useEffect, useState } from "react";

/**
 * Props for the CurrencyEntryField component.
 */
interface CurrencyEntryFieldProps {
  readonly label: string;
  readonly value: number | null;
  readonly setValue?: ((newValue: number | null) => void) | null;
  readonly errorMessage?: string | null;
}

const editPattern = /^-?\d*(?:\.\d{0,2})?$/u;

const formatCurrencyValue = function (value: number): string {
  return value.toLocaleString([], {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
};

const sanitizeCurrencyInput = function (value: string): string {
  return value.replace(/[$,\s]/gu, "");
};

const parseCurrencyValue = function (value: string): number | null {
  if (value === "" || value === "-" || value === "." || value === "-.") {
    return null;
  }

  if (!/^-?(?:\d+|\d*\.\d{1,2})$/u.test(value)) {
    return null;
  }

  const parsedValue = Number(value);
  return Number.isNaN(parsedValue) ? null : parsedValue;
};

/**
 * Component the presents the user with an entry field where they can enter currency values.
 */
const CurrencyEntryField = function ({
  label,
  value,
  setValue = null,
  errorMessage = null,
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
      slotProps={{
        input: {
          disabled: setValue === null,
          startAdornment: <InputAdornment position="start">$</InputAdornment>,
        },
      }}
      onFocus={() => {
        setIsEditing(true);
        setStringValue((currentValue) => sanitizeCurrencyInput(currentValue));
      }}
      onChange={(event) => {
        const nextValue = sanitizeCurrencyInput(event.target.value);
        if (!editPattern.test(nextValue)) {
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
