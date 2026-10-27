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
  readonly maxYears?: number;
}

/**
 * Converts an applied date string to a date field value, treating an empty string as no date.
 */
const toDate = function (date: string): Dayjs | null {
  return date === "" ? null : dayjs(date);
};

const fieldSx = {
  flex: 1,
  flexShrink: 0,
  minWidth: { xs: "100%", sm: 240 },
  "& .MuiFormControl-root": { width: "100%" },
};

/**
 * Renders a pair of date fields that apply a range once a date is picked, the field loses focus, or Enter is pressed,
 * and only when the range is valid.
 */
const DateRangeFilter = function ({
  value,
  onChange,
  maxYears,
}: DateRangeFilterProps): JSX.Element {
  const [startValue, setStartValue] = useState<Dayjs | null>(() =>
    toDate(value.start),
  );
  const [endValue, setEndValue] = useState<Dayjs | null>(() =>
    toDate(value.end),
  );

  useEffect(() => {
    setStartValue(toDate(value.start));
  }, [value.start]);

  useEffect(() => {
    setEndValue(toDate(value.end));
  }, [value.end]);

  const getRangeError = function (
    start: Dayjs | null,
    end: Dayjs | null,
  ): string | null {
    if (start?.isValid() !== true || end?.isValid() !== true) {
      return null;
    }
    if (start.isAfter(end, "day")) {
      return "Start date must be on or before the end date";
    }
    if (
      typeof maxYears === "number" &&
      start.isBefore(end.subtract(maxYears, "year"), "day")
    ) {
      return `Date range can't be longer than ${maxYears} years`;
    }
    return null;
  };
  const rangeError = getRangeError(startValue, endValue);

  const commit = function (start: Dayjs | null, end: Dayjs | null): void {
    if (start?.isValid() !== true || end?.isValid() !== true) {
      // An empty or partly typed date can never be applied, so show the applied range again
      setStartValue(toDate(value.start));
      setEndValue(toDate(value.end));
      return;
    }
    if (getRangeError(start, end) !== null) {
      return;
    }
    const range = {
      start: start.format("YYYY-MM-DD"),
      end: end.format("YYYY-MM-DD"),
    };
    if (range.start !== value.start || range.end !== value.end) {
      onChange(range);
    }
  };

  return (
    <>
      <Box sx={fieldSx}>
        <DateEntryField
          label="Start date"
          value={startValue}
          size="small"
          setValue={setStartValue}
          onCommit={(start) => {
            commit(start, endValue);
          }}
          errorMessage={rangeError}
        />
      </Box>
      <Box sx={fieldSx}>
        <DateEntryField
          label="End date"
          value={endValue}
          size="small"
          setValue={setEndValue}
          onCommit={(end) => {
            commit(startValue, end);
          }}
          // The message shows under the start date, so the end date is only marked as invalid
          errorMessage={rangeError === null ? null : ""}
        />
      </Box>
    </>
  );
};

export default DateRangeFilter;
