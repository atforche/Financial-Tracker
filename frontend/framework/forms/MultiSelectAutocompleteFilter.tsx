import { Autocomplete, Checkbox, TextField } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the MultiSelectAutocompleteFilter component.
 */
interface MultiSelectAutocompleteFilterProps<T> {
  readonly label: string;
  readonly options: readonly T[];
  readonly value: readonly T[];
  readonly onChange: (value: readonly T[]) => void;
  readonly disabled?: boolean;
  readonly placeholder?: string;
  readonly noOptionsText?: string;
  readonly getOptionLabel?: (option: T) => string;
  readonly isOptionEqualToValue?: (option: T, value: T) => boolean;
}

/**
 * Standard searchable multi-select used by page filters.
 */
const MultiSelectAutocompleteFilter = function <T>({
  label,
  options,
  value,
  onChange,
  disabled = false,
  placeholder,
  noOptionsText,
  getOptionLabel,
  isOptionEqualToValue,
}: MultiSelectAutocompleteFilterProps<T>): JSX.Element {
  const optionLabel =
    getOptionLabel ??
    function (option: T): string {
      return String(option);
    };

  return (
    <Autocomplete
      multiple
      disableCloseOnSelect
      size="small"
      options={[...options]}
      value={[...value]}
      disabled={disabled}
      limitTags={1}
      sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
      {...(noOptionsText === undefined ? {} : { noOptionsText })}
      getOptionLabel={optionLabel}
      {...(isOptionEqualToValue === undefined ? {} : { isOptionEqualToValue })}
      slotProps={{
        paper: {
          sx: {
            "& .MuiAutocomplete-listbox": {
              maxHeight: 320,
            },
          },
        },
      }}
      onChange={(_, nextValue) => {
        onChange(nextValue);
      }}
      renderOption={(props, option, { selected }) => (
        <li {...props}>
          <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
          {optionLabel(option)}
        </li>
      )}
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          {...(value.length === 0 && placeholder !== undefined
            ? { placeholder }
            : {})}
        />
      )}
    />
  );
};

export default MultiSelectAutocompleteFilter;
