import type { JSX } from "react";
import { TextField } from "@mui/material";

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
 * Styles for the date fields in the DateRangeFilter component.
 */
const fieldSx = {
  minWidth: { xs: "100%", sm: 180 },
};

/**
 * Renders a pair of native date fields while maintaining an ordered range.
 */
const DateRangeFilter = function ({
  value,
  onChange,
  disabled = false,
}: DateRangeFilterProps): JSX.Element {
  return (
    <>
      <TextField
        size="small"
        label="Start date"
        type="date"
        value={value.start}
        disabled={disabled}
        sx={fieldSx}
        slotProps={{ inputLabel: { shrink: true } }}
        onChange={(event) => {
          const start = event.target.value;
          onChange({ start, end: start > value.end ? start : value.end });
        }}
      />
      <TextField
        size="small"
        label="End date"
        type="date"
        value={value.end}
        disabled={disabled}
        sx={fieldSx}
        slotProps={{ inputLabel: { shrink: true } }}
        onChange={(event) => {
          const end = event.target.value;
          onChange({ start: end < value.start ? end : value.start, end });
        }}
      />
    </>
  );
};

export type { DateRange };
export default DateRangeFilter;
