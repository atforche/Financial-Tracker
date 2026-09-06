import type { SxProps, Theme } from "@mui/material/styles";
import DeleteOutline from "@mui/icons-material/DeleteOutline";
import IconButton from "@mui/material/IconButton";
import type { JSX } from "react";

interface CollectionItemDeleteButtonProps {
  readonly onClick: () => void;
  readonly disabled?: boolean;
  readonly sx?: SxProps<Theme>;
}

/**
 * Displays the standard destructive action for removing a collection item.
 */
const CollectionItemDeleteButton = function ({
  onClick,
  disabled = false,
  sx,
}: CollectionItemDeleteButtonProps): JSX.Element {
  return (
    <IconButton
      aria-label="Delete item"
      color="error"
      size="small"
      disabled={disabled}
      sx={sx}
      onClick={onClick}
    >
      <DeleteOutline fontSize="small" />
    </IconButton>
  );
};

export default CollectionItemDeleteButton;
