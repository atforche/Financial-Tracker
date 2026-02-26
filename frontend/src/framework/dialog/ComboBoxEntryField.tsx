import { Autocomplete, CircularProgress, TextField } from "@mui/material";
import type ApiErrorHandler from "@data/ApiErrorHandler";
import ErrorHelperText from "@framework/dialog/ErrorHelperText";
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
 */
interface ComboBoxEntryFieldProps<T> {
  readonly label: string;
  readonly options: ComboBoxOption<T>[];
  readonly value: ComboBoxOption<T>;
  readonly setValue?: ((newValue: ComboBoxOption<T>) => void) | null;
  readonly loading?: boolean;
  readonly errorHandler?: ApiErrorHandler | null;
  readonly errorKey?: string | null;
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
  errorHandler = null,
  errorKey = null,
}: ComboBoxEntryFieldProps<T>): JSX.Element {
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
          error={(errorHandler?.handleError(errorKey) ?? null) !== null}
          helperText={
            <ErrorHelperText errorHandler={errorHandler} errorKey={errorKey} />
          }
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
