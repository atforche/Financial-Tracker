import { IconButton as MuiIconButton, Tooltip } from "@mui/material";
import type { JSX } from "react/jsx-runtime";

/**
 * Props for the IconButton component.
 */
interface IconButtonProps {
  readonly label: string;
  readonly icon: JSX.Element;
  readonly onClick: () => void;
  readonly disabled?: boolean;
}

/**
 * Component that renders an icon button.
 */
const IconButton = function ({
  label,
  icon,
  onClick,
  disabled = false,
}: IconButtonProps): JSX.Element {
  return (
    <Tooltip title={label}>
      <MuiIconButton
        sx={{
          color: "white",
        }}
        onClick={onClick}
        disabled={disabled}
      >
        {icon}
      </MuiIconButton>
    </Tooltip>
  );
};

export default IconButton;
