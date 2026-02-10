import { IconButton, Tooltip } from "@mui/material";
import type { JSX } from "react/jsx-runtime";

/**
 * Props for the DialogHeaderButton component.
 */
interface DialogHeaderButtonProps {
  readonly label: string;
  readonly icon: JSX.Element;
  readonly onClick: () => void;
}

/**
 * Component that renders a button to be used in a dialog header.
 * @param props - Props for the DialogHeaderButton component.
 * @returns JSX element representing the DialogHeaderButton.
 */
const DialogHeaderButton = function ({
  label,
  icon,
  onClick,
}: DialogHeaderButtonProps): JSX.Element {
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

export default DialogHeaderButton;
