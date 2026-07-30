import {
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from "@mui/material";
import type { JSX } from "react";
import Link from "next/link";
import type { NavigationChildLink } from "./navigationLinkTypes";
import matchesPath from "./matchesPath";

/**
 * Props for the NavigationLinkItem component.
 */
interface NavigationLinkItemProps {
  readonly link: NavigationChildLink;
  readonly pathname: string;
  readonly onNavigate?: (() => void) | undefined;
  readonly isChild?: boolean;
}

/**
 * Displays a single navigation link.
 */
const NavigationLinkItem = function ({
  link,
  pathname,
  onNavigate,
  isChild = false,
}: NavigationLinkItemProps): JSX.Element {
  return (
    <Link
      href={link.href}
      {...(onNavigate === undefined ? {} : { onClick: onNavigate })}
      style={{ textDecoration: "none", color: "inherit" }}
    >
      <ListItem disablePadding>
        <ListItemButton
          selected={matchesPath(pathname, link.href)}
          {...(isChild ? { sx: { pl: 7.5 } } : {})}
        >
          <ListItemIcon
            sx={isChild ? { minWidth: 36 } : { paddingLeft: "15px" }}
          >
            {link.icon}
          </ListItemIcon>
          <ListItemText primary={link.name} />
        </ListItemButton>
      </ListItem>
    </Link>
  );
};

export default NavigationLinkItem;
