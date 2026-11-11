"use client";

import { IconButton, Tooltip } from "@mui/material";
import Add from "@mui/icons-material/Add";
import DeleteOutline from "@mui/icons-material/DeleteOutline";
import EditOutlined from "@mui/icons-material/EditOutlined";
import type { JSX } from "react";
import Link from "next/link";

/**
 * Icon and color for each kind of action.
 */
const actions = {
  add: { Icon: Add, color: "primary" },
  edit: { Icon: EditOutlined, color: "primary" },
  delete: { Icon: DeleteOutline, color: "error" },
} as const;

/**
 * Props for the ActionIconButton component.
 */
type ActionIconButtonProps = {
  readonly action: keyof typeof actions;

  /**
   * What the button does, shown as its tooltip and read by screen readers.
   */
  readonly label: string;
} & (
  | { readonly href: string; readonly onClick?: never }
  | { readonly href?: never; readonly onClick: () => void }
);

/**
 * An add, edit, or delete action shown as just its icon, for places where the surrounding page already says what it
 * acts on.
 */
const ActionIconButton = function ({
  action,
  label,
  href,
  onClick,
}: ActionIconButtonProps): JSX.Element {
  const { Icon, color } = actions[action];
  return (
    <Tooltip title={label}>
      {href === undefined ? (
        <IconButton color={color} aria-label={label} onClick={onClick}>
          <Icon />
        </IconButton>
      ) : (
        <IconButton
          color={color}
          aria-label={label}
          component={Link}
          href={href}
        >
          <Icon />
        </IconButton>
      )}
    </Tooltip>
  );
};

export default ActionIconButton;
