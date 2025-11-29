import { Box, Button, Tooltip } from "@mui/material";
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
    <Box sx={{ verticalAlign: "middle" }}>
      <Tooltip title={label}>
        <Button
          variant="contained"
          startIcon={icon}
          disableElevation
          sx={{
            backgroundColor: "primary",
            border: 1,
            borderColor: "white",
          }}
          onClick={onClick}
        >
          {label}
        </Button>
      </Tooltip>
    </Box>
  );
};

export default ColumnHeaderButton;
