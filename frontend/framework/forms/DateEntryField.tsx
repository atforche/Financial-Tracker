import dayjs, { type Dayjs } from "dayjs";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { DatePicker } from "@mui/x-date-pickers/DatePicker";
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
  readonly errorMessage?: string | null;
  readonly minDate?: Dayjs | null;
  readonly maxDate?: Dayjs | null;
}

/**
 * Component that presents the user with an entry field where they can enter date values.
 */
const DateEntryField = function ({
  label,
  value,
  setValue = null,
  errorMessage = null,
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
        minDate={minDate ?? dayjs("1900-01-01")}
        maxDate={maxDate ?? dayjs("2100-12-31")}
        onError={(internalError) => {
          setInternalErrorMessage(
            internalError === "maxDate" || internalError === "minDate"
              ? `Please pick a date between ${minDate?.format("MM/DD/YYYY")} and ${maxDate?.format("MM/DD/YYYY")}`
              : null,
          );
        }}
        slotProps={{
          textField: {
            error: errorMessage !== null || internalErrorMessage !== null,
            helperText: internalErrorMessage ?? errorMessage,
          },
        }}
      />
    </LocalizationProvider>
  );
};

export default DateEntryField;
