import { InputAdornment, TextField } from "@mui/material";
import { type JSX, useState } from "react";

/**
 * Props for the CurrencyEntryField component.
 */
interface CurrencyEntryFieldProps {
  readonly label: string;
  readonly value: number | null;
  readonly setValue?: ((newValue: number | null) => void) | null;
  readonly errorMessage?: string | null;
}

/**
 * Component the presents the user with an entry field where they can enter currency values.
 */
const CurrencyEntryField = function ({
  label,
  value,
  setValue = null,
  errorMessage = null,
}: CurrencyEntryFieldProps): JSX.Element {
  const [stringValue, setStringValue] = useState<string>(
    value === null
      ? ""
      : value.toLocaleString([], {
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        }),
  );
  if ((value ?? 0) !== Number(stringValue.replaceAll(",", ""))) {
    setStringValue(
      value === null
        ? ""
        : value.toLocaleString([], {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          }),
    );
  }
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
      onChange={(event) => {
        if (event.target.value === "") {
          setStringValue("");
          setValue?.(null);
        } else {
          if (event.target.value.includes(".")) {
            const [prefix, suffix] = event.target.value.split(".");
            setStringValue(
              `${Number(prefix?.replaceAll(",", "")).toLocaleString()}.${suffix?.slice(0, 2)}`,
            );
          } else {
            setStringValue(
              Number(event.target.value.replaceAll(",", "")).toLocaleString(),
            );
          }
          setValue?.(
            Number(
              event.target.value
                .replaceAll(",", "")
                .replace(/\.(\d{2})\d*/u, ".$1"),
            ),
          );
        }
      }}
      error={errorMessage !== null}
      helperText={errorMessage ?? null}
    />
  );
};

export default CurrencyEntryField;
