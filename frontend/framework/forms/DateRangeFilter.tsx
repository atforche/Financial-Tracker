import { type JSX, useEffect, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import { Box } from "@mui/material";
import DateEntryField from "@/framework/forms/DateEntryField";

/**
 * Represents a date range with a start and end date.
 */
interface DateRange {
  readonly start: string;
  readonly end: string;
}

/**
 * Props for the DateRangeFilter component.
 */
interface DateRangeFilterProps {
  readonly value: DateRange;
  readonly onChange: (value: DateRange) => void;
  readonly disabled?: boolean;
}

/**
 * Renders a pair of date fields while maintaining an ordered range.
 */
const DateRangeFilter = function ({
  value,
  onChange,
  disabled = false,
}: DateRangeFilterProps): JSX.Element {
  const [startValue, setStartValue] = useState<Dayjs | null>(() =>
    value.start === "" ? null : dayjs(value.start),
  );
  const [endValue, setEndValue] = useState<Dayjs | null>(() =>
    value.end === "" ? null : dayjs(value.end),
  );

  useEffect(() => {
    setStartValue(value.start === "" ? null : dayjs(value.start));
  }, [value.start]);

  useEffect(() => {
    setEndValue(value.end === "" ? null : dayjs(value.end));
  }, [value.end]);

  const formatDate = function (date: Dayjs | null): string {
    return date?.isValid() === true ? date.format("YYYY-MM-DD") : "";
  };

  return (
    <>
      <Box
        sx={{
          flex: 1,
          minWidth: 0,
          "& .MuiFormControl-root": { width: "100%", minWidth: 0 },
        }}
      >
        <DateEntryField
          label="Start date"
          value={startValue}
          disabled={disabled}
          size="small"
          setValue={(nextStart) => {
            setStartValue(nextStart);
            if (nextStart?.isValid() !== true) {
              return;
            }
            const start = formatDate(nextStart);
            onChange({ start, end: start > value.end ? start : value.end });
          }}
        />
      </Box>
      <Box
        sx={{
          flex: 1,
          minWidth: 0,
          "& .MuiFormControl-root": { width: "100%", minWidth: 0 },
        }}
      >
        <DateEntryField
          label="End date"
          value={endValue}
          disabled={disabled}
          size="small"
          setValue={(nextEnd) => {
            setEndValue(nextEnd);
            if (nextEnd?.isValid() !== true) {
              return;
            }
            const end = formatDate(nextEnd);
            onChange({ start: end < value.start ? end : value.start, end });
          }}
        />
      </Box>
    </>
  );
};

export type { DateRange };
export default DateRangeFilter;
