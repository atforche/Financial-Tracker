import { IconButton, Tooltip } from "@mui/material";
import type { JSX } from "react/jsx-runtime";

/**
 * Props for ColumnButton component.
 */
interface ColumnButtonProps {
  readonly label: string;
  readonly icon: JSX.Element;
  readonly onClick: (() => void) | null;
  readonly hidden?: boolean;
}

/**
 * Component that renders a button to be used in a column.
 */
const ColumnButton = function ({
  label,
  icon,
  onClick,
  hidden = false,
}: ColumnButtonProps): JSX.Element {
  return (
    <Tooltip
      title={label}
      className="column-button"
      sx={{ visibility: hidden ? "hidden" : "visible" }}
    >
      <IconButton key={label} size="small" onClick={() => onClick?.()}>
        {icon}
      </IconButton>
    </Tooltip>
  );
};

export default ColumnButton;
