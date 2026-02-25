import {
  Checkbox,
  FormControl,
  FormControlLabel,
  FormHelperText,
} from "@mui/material";
import type ApiErrorHandler from "@data/ApiErrorHandler";
import ErrorHelperText from "@framework/dialog/ErrorHelperText";
import type { JSX } from "react";

/**
 * Props for the BooleanEntryField component.
 * @param label - Label for this Boolean Entry Field.
 * @param setValue - Callback to update the value in this Boolean Entry Field. If null, this field is read-only.
 * @param value - Current value for this Boolean Entry Field.
 */
interface BooleanEntryFieldProps {
  readonly label: string;
  readonly value: boolean;
  readonly setValue?: ((newValue: boolean) => void) | null;
  readonly errorHandler?: ApiErrorHandler | null;
  readonly errorKey?: string | null;
}

/**
 * Component the presents the user with an entry field where they can enter boolean values.
 * @param props - Props for the BooleanEntryField component.
 * @returns JSX element representing the BooleanEntryField component.
 */
const BooleanEntryField = function ({
  label,
  value,
  setValue = null,
  errorHandler = null,
  errorKey = null,
}: BooleanEntryFieldProps): JSX.Element {
  return (
    <FormControl error={(errorHandler?.handleError(errorKey) ?? null) !== null}>
      <FormControlLabel
        control={
          <Checkbox
            checked={value}
            onChange={(event) => {
              setValue?.(event.target.checked);
            }}
          />
        }
        label={label}
        labelPlacement="start"
        sx={{ justifyContent: "left" }}
      />
      <FormHelperText>
        <ErrorHelperText errorHandler={errorHandler} errorKey={errorKey} />
      </FormHelperText>
    </FormControl>
  );
};

export default BooleanEntryField;
