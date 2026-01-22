import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import type { ApiErrorDetail } from "@data/ApiError";
import { DatePicker } from "@mui/x-date-pickers/DatePicker";
import type { Dayjs } from "dayjs";
import type { JSX } from "react/jsx-runtime";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { useState } from "react";

/**
 * Props for the DateEntryField component.
 * @param label - Label for this Date Entry Field.
 * @param value - Current value for this Date Entry Field.
 * @param setValue - Callback to update the value in this Date Entry Field. If null, this field is read-only.
 * @param error - Error detail to display for this Date Entry Field, if any.
 * @param minDate - Minimum selectable date for this Date Entry Field, if any.
 * @param maxDate - Maximum selectable date for this Date Entry Field, if any.
 */
interface DateEntryFieldProps {
  readonly label: string;
  readonly value: Dayjs | null;
  readonly setValue?: ((newValue: Dayjs | null) => void) | null;
  readonly error?: ApiErrorDetail | null;
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
  error = null,
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
            error: error !== null || internalErrorMessage !== null,
            helperText: error?.description ?? internalErrorMessage,
          },
        }}
      />
    </LocalizationProvider>
  );
};

export default DateEntryField;
