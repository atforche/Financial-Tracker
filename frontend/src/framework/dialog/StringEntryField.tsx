import type ApiErrorHandler from "@data/ApiErrorHandler";
import ErrorHelperText from "@framework/dialog/ErrorHelperText";
import type { JSX } from "react";
import { TextField } from "@mui/material";

/**
 * Props for the StringEntryField component.
 * @param label - Label for this String Entry Field.
 * @param setValue - Callback to update the value in this String Entry Field. If null, this field is read-only.
 * @param value - Current value for this String Entry Field.
 */
interface StringEntryFieldProps {
  readonly label: string;
  readonly value: string;
  readonly setValue?: ((newValue: string) => void) | null;
  readonly errorHandler?: ApiErrorHandler | null;
  readonly errorKey?: string | null;
}

/**
 * Component the presents the user with an entry field where they can enter string values.
 * @param props - Props for the StringEntryField component.
 * @returns JSX element representing the StringEntryField component.
 */
const StringEntryField = function ({
  label,
  value,
  setValue = null,
  errorHandler = null,
  errorKey = null,
}: StringEntryFieldProps): JSX.Element {
  return (
    <TextField
      label={label}
      variant="outlined"
      value={value}
      slotProps={{
        input: {
          readOnly: setValue === null,
        },
      }}
      onChange={(event) => setValue?.(event.target.value)}
      error={(errorHandler?.handleError(errorKey) ?? null) !== null}
      helperText={
        <ErrorHelperText errorHandler={errorHandler} errorKey={errorKey} />
      }
    />
  );
};

export default StringEntryField;
