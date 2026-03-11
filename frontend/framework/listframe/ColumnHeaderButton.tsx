import { IconButton, Tooltip } from "@mui/material";
import type { JSX } from "react/jsx-runtime";

/**
 * Props for the ColumnHeaderButton component.
 */
interface ColumnHeaderButtonProps {
  readonly label: string;
  readonly icon: JSX.Element;
  readonly onClick: () => void;
}

/**
 * Component that renders a button to be used in a column header.
 * @param props - Props for the ColumnHeaderButton component.
 * @returns JSX element representing the ColumnHeaderButton.
 */
const ColumnHeaderButton = function ({
  label,
  icon,
  onClick,
}: ColumnHeaderButtonProps): JSX.Element {
  return (
    <Tooltip title={label}>
      <IconButton
        sx={{
          color: "white",
        }}
        onClick={onClick}
      >
        {icon}
      </IconButton>
    </Tooltip>
  );
};

export default ColumnHeaderButton;
