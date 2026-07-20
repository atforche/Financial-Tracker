import dayjs, { type Dayjs } from "dayjs";
import { DatePicker } from "@mui/x-date-pickers/DatePicker";
import type { JSX } from "react/jsx-runtime";
import ReadOnlyField from "@/framework/forms/ReadOnlyField";
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
  readonly disabled?: boolean;
}

const defaultMinDate = dayjs("1900-01-01");
const defaultMaxDate = dayjs("2100-12-31");

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
  disabled = false,
}: DateEntryFieldProps): JSX.Element {
  const [internalErrorMessage, setInternalErrorMessage] = useState<
    string | null
  >(null);
  const effectiveMinDate = minDate ?? defaultMinDate;
  const effectiveMaxDate = maxDate ?? defaultMaxDate;

  if (setValue === null && !disabled) {
    return (
      <ReadOnlyField
        label={label}
        value={value === null ? null : value.format("MM/DD/YYYY")}
      />
    );
  }

  return (
    <DatePicker
      label={label}
      value={value}
      disabled={disabled}
      readOnly={setValue === null}
      onChange={(newValue: Dayjs | null) => setValue?.(newValue)}
      minDate={effectiveMinDate}
      maxDate={effectiveMaxDate}
      onError={(internalError) => {
        setInternalErrorMessage(
          internalError === "maxDate" || internalError === "minDate"
            ? `Please pick a date between ${effectiveMinDate.format("MM/DD/YYYY")} and ${effectiveMaxDate.format("MM/DD/YYYY")}`
            : internalError === "invalidDate"
              ? "Please enter a valid date"
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
  );
};

export default DateEntryField;
