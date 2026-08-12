import { Box, IconButton, Stack } from "@mui/material";
import type { JSX, ReactNode } from "react";
import { DeleteOutline } from "@mui/icons-material";

/**
 * Props for the PayrollItemShell component.
 */
interface PayrollItemShellProps {
  readonly children: ReactNode;
  readonly onDelete: (() => void) | null;
}

/**
 * Frames an individual payroll item and its optional delete action.
 */
const PayrollItemShell = function ({
  children,
  onDelete,
}: PayrollItemShellProps): JSX.Element {
  return (
    <Box
      sx={{
        position: "relative",
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: 2,
      }}
    >
      <Stack spacing={1.5} sx={{ pr: onDelete === null ? 0 : 5 }}>
        {children}
      </Stack>
      {onDelete === null ? null : (
        <IconButton
          color="error"
          onClick={onDelete}
          sx={{ position: "absolute", top: 24, right: 8 }}
        >
          <DeleteOutline />
        </IconButton>
      )}
    </Box>
  );
};

export default PayrollItemShell;
