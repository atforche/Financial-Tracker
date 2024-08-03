import { Autocomplete, TextField } from "@mui/material";
import DialogMode from "@core/fieldValues/DialogMode";
import type { EntryFieldProps } from "@ui/framework/EntryFieldProps";
import type FieldValue from "@core/fieldValues/FieldValue";

interface FieldValueEntryFieldProps<T extends FieldValue>
  extends EntryFieldProps<T> {
  fieldValueCollection: T[];
}

/**
 * Component the presents the user with a combo box where they can select from a set of Enum values.
 * @param {EnumEntryFieldProps} props - Props for the EnumEntryField component.
 * @returns {JSX.Element} JSX element representing the EnumEntryField.
 */
const FieldValueEntryField = function <T extends FieldValue>({
  dialogMode,
  label,
  setValue,
  value,
  fieldValueCollection,
  isReadOnly = null,
}: FieldValueEntryFieldProps<T>): JSX.Element {
  const options = fieldValueCollection;
  const width = 300;

  // If the dialog mode is View, all fields will be closed so display them as read-only.
  if (dialogMode === DialogMode.View) {
    return (
      <Autocomplete
        readOnly
        options={options}
        value={value}
        sx={{ width: { width } }}
        renderInput={(params: object) => (
          <TextField {...params} label={label} />
        )}
      />
    );
  }
  // If only this field is read-only, make this field disabled to clearly differentiate it from the other fields.
  if (isReadOnly?.() ?? false) {
    return (
      <Autocomplete
        disabled
        options={options}
        value={value}
        sx={{ width: { width } }}
        renderInput={(params: object) => (
          <TextField {...params} label={label} />
        )}
      />
    );
  }
  return (
    <Autocomplete
      disableClearable
      options={options}
      value={value}
      sx={{ width: { width } }}
      renderInput={(params: object) => <TextField {...params} label={label} />}
      getOptionLabel={(fieldValue) => fieldValue.toString()}
      onChange={(_, newValue) => {
        setValue(newValue);
      }}
    />
  );
};

export default FieldValueEntryField;
