import { Box, Typography } from "@mui/material";
import type { JSX } from "react";

/**
 * Displays an empty state for a read-only payroll section.
 */
const PayrollEmptyState = function (): JSX.Element {
  return (
    <Box
      sx={{
        border: "1px dashed",
        borderColor: "divider",
        borderRadius: 3,
        p: 2,
        textAlign: "center",
      }}
    >
      <Typography variant="body2" color="text.secondary">
        No items available.
      </Typography>
    </Box>
  );
};

export default PayrollEmptyState;
