import { IconButton, Tooltip } from "@mui/material";
import type { JSX } from "react/jsx-runtime";

/**
 * Props for the ColumnHeaderButton component.
 */
interface ColumnHeaderButtonProps {
  readonly label: string;
  readonly icon: JSX.Element;
  readonly onClick: () => void;
  readonly disabled?: boolean;
}

/**
 * Component that renders a button to be used in a column header.
 */
const ColumnHeaderButton = function ({
  label,
  icon,
  onClick,
  disabled = false,
}: ColumnHeaderButtonProps): JSX.Element {
  return (
    <Tooltip title={label}>
      <IconButton
        sx={{
          color: "white",
        }}
        onClick={onClick}
        disabled={disabled}
      >
        {icon}
      </IconButton>
    </Tooltip>
  );
};

export default ColumnHeaderButton;
