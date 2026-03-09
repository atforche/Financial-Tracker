import { IconButton, Tooltip } from "@mui/material";
import type { JSX } from "react/jsx-runtime";

/**
 * Props for ColumnButton component.
 * @param label - The label for the button.
 * @param icon - The icon to display on the button.
 * @param onClick - The function to call when the button is clicked.
 */
interface ColumnButtonProps {
  readonly label: string;
  readonly icon: JSX.Element;
}

/**
 * Component that renders a button to be used in a column.
 * @param props - Props for the ColumnButton component.
 * @returns JSX element representing the ColumnButton component.
 */
const ColumnButton = function ({
  label,
  icon,
}: ColumnButtonProps): JSX.Element {
  return (
    <Tooltip title={label} className="column-button">
      <IconButton key={label} size="small">
        {icon}
      </IconButton>
    </Tooltip>
  );
};

export default ColumnButton;
