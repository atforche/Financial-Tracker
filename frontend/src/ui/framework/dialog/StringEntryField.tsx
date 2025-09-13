import DialogMode from "@core/fieldValues/DialogMode";
import type { NullableEntryFieldProps } from "@ui/framework/dialog/EntryFieldProps";
import { TextField } from "@mui/material";
import { toNullIfBlank } from "@core/utils";

/**
 * Component the presents the user with an entry field where they can enter string values.
 * @param {NullableEntryFieldProps<string>} props - Props for the StringEntryField component.
 * @returns {JSX.Element} JSX element representing the StringEntryField component.
 */
const StringEntryField = function ({
  dialogMode,
  label,
  setValue,
  value,
  isReadOnly = null,
}: NullableEntryFieldProps<string>): JSX.Element {
  // If the dialog mode is View, all fields will be closed so display them as read-only.
  if (dialogMode === DialogMode.View) {
    return (
      <TextField
        label={label}
        variant="outlined"
        value={value}
        InputProps={{ readOnly: true }}
      />
    );
  }
  // If only this field is read-only, make this field disabled to clearly differentiate it from the other fields.
  if (isReadOnly?.() ?? false) {
    <TextField
      label={label}
      variant="outlined"
      value={value}
      InputProps={{ disabled: true }}
    />;
  }
  return (
    <TextField
      label={label}
      variant="outlined"
      value={value}
      onChange={(event) => {
        setValue(toNullIfBlank(event.target.value));
      }}
    />
  );
};

export default StringEntryField;
