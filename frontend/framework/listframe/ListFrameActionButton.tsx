import { IconButton, type IconButtonProps, Tooltip } from "@mui/material";
import type { JSX, MouseEvent } from "react";

/**
 * Props for the ListFrameActionButton component.
 */
type ListFrameActionButtonProps = Omit<
  IconButtonProps,
  "aria-label" | "onClick"
> & {
  readonly ariaLabel: string;
  readonly label?: string;
  readonly onClick: (event: MouseEvent<HTMLButtonElement>) => void;
};

/**
 * Renders an accessible action button without triggering its containing row.
 */
const ListFrameActionButton = function ({
  label,
  ariaLabel,
  onClick,
  ...props
}: ListFrameActionButtonProps): JSX.Element {
  return (
    <Tooltip title={label ?? ariaLabel}>
      <IconButton
        {...props}
        aria-label={ariaLabel}
        onClick={(event) => {
          event.stopPropagation();
          onClick(event);
        }}
      />
    </Tooltip>
  );
};

export default ListFrameActionButton;
