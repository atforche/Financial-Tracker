import { Box, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";

/**
 * Props for the ReadOnlyField component.
 */
interface ReadOnlyFieldProps {
  readonly label: string;
  readonly value: ReactNode;
  readonly emptyValue?: ReactNode;
}

/**
 * Displays a labeled value without presenting it as an input control.
 */
const ReadOnlyField = function ({
  label,
  value,
  emptyValue = "—",
}: ReadOnlyFieldProps): JSX.Element {
  const displayValue =
    value === null || typeof value === "undefined" || value === ""
      ? emptyValue
      : value;

  return (
    <Box sx={{ minWidth: 0, py: 0.75 }}>
      <Typography
        component="div"
        variant="caption"
        color="text.secondary"
        sx={{ mb: 0.25 }}
      >
        {label}
      </Typography>
      <Typography
        component="div"
        variant="body1"
        sx={{ fontWeight: 500, overflowWrap: "anywhere" }}
      >
        {displayValue}
      </Typography>
    </Box>
  );
};

export default ReadOnlyField;
