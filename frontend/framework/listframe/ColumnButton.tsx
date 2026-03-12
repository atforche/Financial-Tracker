import { IconButton, Tooltip } from "@mui/material";
import type { JSX } from "react/jsx-runtime";

/**
 * Props for ColumnButton component.
 */
interface ColumnButtonProps {
  readonly label: string;
  readonly icon: JSX.Element;
  readonly onClick: (() => void) | null;
}

/**
 * Component that renders a button to be used in a column.
 */
const ColumnButton = function ({
  label,
  icon,
  onClick,
}: ColumnButtonProps): JSX.Element {
  return (
    <Tooltip title={label} className="column-button">
      <IconButton key={label} size="small" onClick={() => onClick?.()}>
        {icon}
      </IconButton>
    </Tooltip>
  );
};

export default ColumnButton;
