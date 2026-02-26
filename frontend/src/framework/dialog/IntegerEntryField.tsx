import type ApiErrorHandler from "@data/ApiErrorHandler";
import ErrorHelperText from "@framework/dialog/ErrorHelperText";
import type { JSX } from "react";
import { TextField } from "@mui/material";

/**
 * Props for the IntegerEntryField component.
 * @param label - Label for this Integer Entry Field.
 * @param setValue - Callback to update the value in this Integer Entry Field. If null, this field is read-only.
 * @param value - Current value for this Integer Entry Field.
 */
interface IntegerEntryFieldProps {
  readonly label: string;
  readonly value: number | null;
  readonly setValue?: ((newValue: number | null) => void) | null;
  readonly errorHandler?: ApiErrorHandler | null;
  readonly errorKey?: string | null;
}

/**
 * Component the presents the user with an entry field where they can enter integer values.
 * @param props - Props for the IntegerEntryField component.
 * @returns JSX element representing the IntegerEntryField component.
 */
const IntegerEntryField = function ({
  label,
  value,
  setValue = null,
  errorHandler = null,
  errorKey = null,
}: IntegerEntryFieldProps): JSX.Element {
  return (
    <TextField
      label={label}
      variant="outlined"
      value={value}
      slotProps={{
        input: {
          readOnly: setValue === null,
          inputMode: "numeric",
        },
        htmlInput: {
          pattern: "[0-9]*",
        },
      }}
      onChange={(event) =>
        setValue?.(
          event.target.value === "" ? null : Number(event.target.value),
        )
      }
      error={(errorHandler?.handleError(errorKey) ?? null) !== null}
      helperText={
        <ErrorHelperText errorHandler={errorHandler} errorKey={errorKey} />
      }
    />
  );
};

export default IntegerEntryField;
