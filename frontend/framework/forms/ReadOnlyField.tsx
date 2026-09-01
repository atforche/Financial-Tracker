import { Box, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import type { SxProps, Theme } from "@mui/material/styles";

/**
 * Props for the ReadOnlyField component.
 */
interface ReadOnlyFieldProps {
  readonly label: string;
  readonly value: ReactNode;
  readonly emptyValue?: ReactNode;
  readonly sx?: SxProps<Theme>;
}

/**
 * Displays a labeled value without presenting it as an input control.
 */
const ReadOnlyField = function ({
  label,
  value,
  emptyValue = "—",
  sx,
}: ReadOnlyFieldProps): JSX.Element {
  const displayValue =
    value === null || typeof value === "undefined" || value === ""
      ? emptyValue
      : value;

  return (
    <Box sx={[{ minWidth: 0, py: 0.75 }, ...(sx === undefined ? [] : [sx])]}>
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
