import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import type ApiErrorHandler from "@data/ApiErrorHandler";
import { DatePicker } from "@mui/x-date-pickers/DatePicker";
import type { Dayjs } from "dayjs";
import ErrorHelperText from "@framework/dialog/ErrorHelperText";
import type { JSX } from "react/jsx-runtime";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { useState } from "react";

/**
 * Props for the DateEntryField component.
 */
interface DateEntryFieldProps {
  readonly label: string;
  readonly value: Dayjs | null;
  readonly setValue?: ((newValue: Dayjs | null) => void) | null;
  readonly errorHandler?: ApiErrorHandler | null;
  readonly errorKey?: string | null;
  readonly minDate?: Dayjs | null;
  readonly maxDate?: Dayjs | null;
}

/**
 * Component that presents the user with an entry field where they can enter date values.
 * @param props - Props for the DateEntryField component.
 * @returns JSX element representing the DateEntryField component.
 */
const DateEntryField = function ({
  label,
  value,
  setValue = null,
  errorHandler = null,
  errorKey = null,
  minDate = null,
  maxDate = null,
}: DateEntryFieldProps): JSX.Element {
  const [internalErrorMessage, setInternalErrorMessage] = useState<
    string | null
  >(null);
  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <DatePicker
        label={label}
        value={value}
        readOnly={setValue === null}
        onChange={(newValue: Dayjs | null) => setValue?.(newValue)}
        minDate={minDate}
        maxDate={maxDate}
        onError={(internalError) => {
          setInternalErrorMessage(
            internalError === "maxDate" || internalError === "minDate"
              ? `Please pick a date between ${minDate?.format("MM/DD/YYYY")} and ${maxDate?.format("MM/DD/YYYY")}`
              : null,
          );
        }}
        slotProps={{
          textField: {
            error:
              (errorHandler?.handleError(errorKey) ?? null) !== null ||
              internalErrorMessage !== null,
            helperText: internalErrorMessage ?? (
              <ErrorHelperText
                errorHandler={errorHandler}
                errorKey={errorKey}
              />
            ),
          },
        }}
      />
    </LocalizationProvider>
  );
};

export default DateEntryField;
