import { AddCircleOutline } from "@mui/icons-material";
import { Button } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the PayrollAddButton component.
 */
interface PayrollAddButtonProps {
  readonly label: string;
  readonly onClick: (() => void) | null;
}

/**
 * Displays an add action when a payroll section is editable.
 */
const PayrollAddButton = function ({
  label,
  onClick,
}: PayrollAddButtonProps): JSX.Element | null {
  return onClick === null ? null : (
    <Button
      variant="outlined"
      size="small"
      startIcon={<AddCircleOutline />}
      onClick={onClick}
    >
      {label}
    </Button>
  );
};

export default PayrollAddButton;
