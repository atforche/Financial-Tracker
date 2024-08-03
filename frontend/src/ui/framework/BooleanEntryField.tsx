import { Checkbox, FormControlLabel } from "@mui/material";
import DialogMode from "@core/fieldValues/DialogMode";
import type { EntryFieldProps } from "@ui/framework/EntryFieldProps";

/**
 * Component that presents the user with an entry field where they can enter boolean values.
 * @param {EntryFieldProps<boolean>} props - Props for the BooleanEntryField component.
 * @returns {JSX.Element} JSX element representing the BooleanEntryField component.
 */
const BooleanEntryField = function ({
  dialogMode,
  label,
  setValue,
  value,
  isReadOnly = null,
}: EntryFieldProps<boolean>): JSX.Element {
  const labelPlacement = "start";
  const justifyContent = "left";

  // If the dialog mode is view, all the fields will be closed so display them as read-only
  if (dialogMode === DialogMode.View) {
    return (
      <FormControlLabel
        control={<Checkbox checked={value} readOnly />}
        label={label}
        labelPlacement={labelPlacement}
        sx={{ justifyContent }}
      />
    );
  }
  // If only this field is read-only, make this field disabled to clearly differentiate it from the other fields.
  if (isReadOnly?.() ?? false) {
    return (
      <FormControlLabel
        control={<Checkbox checked={value} disabled />}
        label={label}
        labelPlacement={labelPlacement}
        sx={{ justifyContent }}
      />
    );
  }
  return (
    <FormControlLabel
      control={
        <Checkbox
          checked={value}
          onChange={(event) => {
            setValue(event.target.checked);
          }}
        />
      }
      label={label}
      labelPlacement={labelPlacement}
      sx={{ justifyContent }}
    />
  );
};

export default BooleanEntryField;
