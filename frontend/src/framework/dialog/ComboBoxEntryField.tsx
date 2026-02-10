import type { ApiError, ApiErrorDetail } from "@data/ApiError";
import { Autocomplete, CircularProgress, TextField } from "@mui/material";
import type { JSX } from "react";

/**
 * Interface representing a Combo Box option.
 * @param label - Label for this option.
 * @param value - Value for this option.
 */
interface ComboBoxOption<T> {
  readonly label: string;
  readonly value: T | null;
}

/**
 * Props for the ComboBoxEntryField component.
 * @param label - Label for this Combo Box Entry Field.
 * @param options - Options to present in this Combo Box Entry Field.
 * @param getOptionLabel - Function to retrieve the label for a given option.
 * @param value - Current value for this Combo Box Entry Field.
 * @param setValue - Callback to update the value in this Combo Box Entry Field. If null, this field is read-only.
 * @param loading - Whether or not this Combo Box Entry Field is in a loading state.
 * @param error - Optional error to display for this Combo Box Entry Field.
 */
interface ComboBoxEntryFieldProps<T> {
  readonly label: string;
  readonly options: ComboBoxOption<T>[];
  readonly value: ComboBoxOption<T>;
  readonly setValue?: ((newValue: ComboBoxOption<T>) => void) | null;
  readonly loading?: boolean;
  readonly error?: ApiError | ApiErrorDetail | null;
}

/**
 * Component the presents the user with an entry field where they can enter string values.
 * @param props - Props for the ComboBoxEntryField component.
 * @returns JSX element representing the ComboBoxEntryField component.
 */
const ComboBoxEntryField = function <T>({
  label,
  options,
  value,
  setValue = null,
  loading = false,
  error = null,
}: ComboBoxEntryFieldProps<T>): JSX.Element {
  let errorMessage = "";
  if (error !== null) {
    if ("message" in error) {
      errorMessage = error.message;
    } else {
      errorMessage = error.description;
    }
  }

  return (
    <Autocomplete
      className="combo-box-entry-field"
      disabled={setValue === null}
      disableClearable
      options={options}
      value={value}
      renderInput={(params) => (
        // @ts-expect-error: The params type from MUI is not correct, even though it comes from their examples and works at runtime.
        <TextField
          {...params}
          label={label}
          error={error !== null}
          helperText={errorMessage}
          slotProps={{
            input: {
              ...params.InputProps,
              endAdornment: (
                <>
                  {loading ? (
                    <CircularProgress color="inherit" size={20} />
                  ) : null}
                  {params.InputProps.endAdornment}
                </>
              ),
            },
          }}
        />
      )}
      loading={loading}
      onChange={(_, newValue) => {
        setValue?.(newValue);
      }}
    />
  );
};

export { type ComboBoxOption, ComboBoxEntryField };
