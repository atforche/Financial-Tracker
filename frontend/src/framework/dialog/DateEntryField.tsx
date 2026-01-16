import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import type { ApiErrorDetail } from "@data/ApiError";
import { DatePicker } from "@mui/x-date-pickers/DatePicker";
import type { Dayjs } from "dayjs";
import type { JSX } from "react/jsx-runtime";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";

/**
 * Props for the DateEntryField component.
 * @param label - Label for this Date Entry Field.
 * @param setValue - Callback to update the value in this Date Entry Field. If null, this field is read-only.
 * @param value - Current value for this Date Entry Field.
 */
interface DateEntryFieldProps {
  readonly label: string;
  readonly value: Dayjs | null;
  readonly setValue?: ((newValue: Dayjs | null) => void) | null;
  readonly error?: ApiErrorDetail | null;
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
}: DateEntryFieldProps): JSX.Element {
  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <DatePicker
        label={label}
        value={value}
        readOnly={setValue === null}
        onChange={(newValue: Dayjs | null) => setValue?.(newValue)}
        slotProps={{
          textField: {
            error: error !== null,
            helperText: error?.description ?? "",
          },
        }}
      />
    </LocalizationProvider>
  );
};

export default DateEntryField;
