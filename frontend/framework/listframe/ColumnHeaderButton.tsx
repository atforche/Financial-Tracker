import { IconButton, Tooltip } from "@mui/material";
import type { JSX } from "react/jsx-runtime";

/**
 * Props for the ColumnHeaderButton component.
 * @param label - The label for the button.
 * @param icon - The icon to display on the button.
 * @param onClick - The function to call when the button is clicked.
 */
interface ColumnHeaderButtonProps {
  readonly label: string;
  readonly icon: JSX.Element;
}

/**
 * Component that renders a button to be used in a column header.
 * @param props - Props for the ColumnHeaderButton component.
 * @returns JSX element representing the ColumnHeaderButton.
 */
const ColumnHeaderButton = function ({
  label,
  icon,
}: ColumnHeaderButtonProps): JSX.Element {
  return (
    <Tooltip title={label}>
      <IconButton
        sx={{
          color: "white",
        }}
      >
        {icon}
      </IconButton>
    </Tooltip>
  );
};

export default ColumnHeaderButton;
