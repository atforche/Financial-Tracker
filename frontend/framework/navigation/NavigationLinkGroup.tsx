import {
  Collapse,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from "@mui/material";
import ExpandLess from "@mui/icons-material/ExpandLess";
import ExpandMore from "@mui/icons-material/ExpandMore";
import type { JSX } from "react";
import Link from "next/link";
import type { NavigationLink } from "./navigationLinkTypes";
import NavigationLinkItem from "./NavigationLinkItem";
import matchesPath from "./matchesPath";

/**
 * Props for the NavigationLinkGroup component.
 */
interface NavigationLinkGroupProps {
  readonly isTemporary: boolean;
  readonly link: NavigationLink & {
    childLinks: NonNullable<NavigationLink["childLinks"]>;
  };
  readonly pathname: string;
  readonly isExpanded: boolean;
  readonly onToggle: () => void;
  readonly onNavigate?: (() => void) | undefined;
}

/**
 * Displays a navigation link and its collapsible child links.
 */
const NavigationLinkGroup = function ({
  isTemporary,
  link,
  pathname,
  isExpanded,
  onToggle,
  onNavigate,
}: NavigationLinkGroupProps): JSX.Element {
  const isSelected =
    matchesPath(pathname, link.href) ||
    link.childLinks.some((child) => matchesPath(pathname, child.href));

  return (
    <div>
      <ListItem
        disablePadding
        secondaryAction={
          <IconButton
            edge="end"
            aria-label={`${isExpanded ? "Collapse" : "Expand"} ${link.name}`}
            onClick={onToggle}
          >
            {isExpanded ? <ExpandLess /> : <ExpandMore />}
          </IconButton>
        }
      >
        {isTemporary ? (
          <ListItemButton
            aria-expanded={isExpanded}
            onClick={onToggle}
            selected={isSelected}
            sx={{ width: "100%", pr: 7.5 }}
          >
            <ListItemIcon sx={{ paddingLeft: "15px" }}>
              {link.icon}
            </ListItemIcon>
            <ListItemText primary={link.name} />
          </ListItemButton>
        ) : (
          <Link
            href={link.href}
            {...(onNavigate === undefined ? {} : { onClick: onNavigate })}
            style={{
              textDecoration: "none",
              color: "inherit",
              width: "100%",
            }}
          >
            <ListItemButton
              selected={isSelected}
              sx={{ width: "100%", pr: 7.5 }}
            >
              <ListItemIcon sx={{ paddingLeft: "15px" }}>
                {link.icon}
              </ListItemIcon>
              <ListItemText primary={link.name} />
            </ListItemButton>
          </Link>
        )}
      </ListItem>
      <Collapse in={isExpanded} timeout="auto" unmountOnExit>
        <List disablePadding>
          {link.childLinks.map((childLink) => (
            <NavigationLinkItem
              key={childLink.name}
              link={childLink}
              pathname={pathname}
              onNavigate={onNavigate}
              isChild
            />
          ))}
        </List>
      </Collapse>
    </div>
  );
};

export default NavigationLinkGroup;
