import { Autocomplete, TextField } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the ComboBoxEntryField component.
 * @param label - Label for this Combo Box Entry Field.
 * @param setValue - Callback to update the value in this Combo Box Entry Field. If null, this field is read-only.
 * @param value - Current value for this Combo Box Entry Field.
 */
interface ComboBoxEntryFieldProps<T extends object | string> {
  readonly label: string;
  readonly options: T[];
  readonly getOptionLabel: (option: T) => string;
  readonly value: T;
  readonly setValue?: ((newValue: T) => void) | null;
}

/**
 * Component the presents the user with an entry field where they can enter string values.
 * @param props - Props for the ComboBoxEntryField component.
 * @returns JSX element representing the ComboBoxEntryField component.
 */
const ComboBoxEntryField = function <T extends object | string>({
  label,
  options,
  getOptionLabel,
  value,
  setValue = null,
}: ComboBoxEntryFieldProps<T>): JSX.Element {
  return (
    <Autocomplete
      readOnly={setValue === null}
      disableClearable
      options={options}
      value={value}
      renderInput={(params: object) => <TextField {...params} label={label} />}
      getOptionLabel={getOptionLabel}
      onChange={(_, newValue) => {
        setValue?.(newValue);
      }}
    />
  );
};

export default ComboBoxEntryField;
