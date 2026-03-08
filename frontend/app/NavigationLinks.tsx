"use client";

import {
  AccountBalance,
  Assessment,
  CalendarMonth,
  GridView,
} from "@mui/icons-material";
import {
  Box,
  Divider,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from "@mui/material";
import type { JSX } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";

/**
 * Interface representing a navigation link in the application.
 */
interface NavigationLink {
  name: string;
  href: string;
  icon: JSX.Element;
}

/**
 * Collection of links to be displayed in the application's navigation menu.
 */
const links: NavigationLink[] = [
  { name: "Overview", href: "/", icon: <GridView /> },
  {
    name: "Accounting Periods",
    href: "/accounting-periods",
    icon: <CalendarMonth />,
  },
  { name: "Accounts", href: "/accounts", icon: <AccountBalance /> },
  { name: "Funds", href: "/funds", icon: <Assessment /> },
];

/**
 * Components displaying the navigation links for the application.
 */
const NavigationLinks = function (): JSX.Element {
  const pathname = usePathname();
  return (
    <Box sx={{ overflow: "auto" }}>
      <Divider />
      <List>
        {links.map((link) => (
          <Link
            key={link.name}
            href={link.href}
            style={{ textDecoration: "none", color: "inherit" }}
          >
            <ListItem disablePadding>
              <ListItemButton selected={pathname === link.href}>
                <ListItemIcon sx={{ paddingLeft: "15px" }}>
                  {link.icon}
                </ListItemIcon>
                <ListItemText primary={link.name} />
              </ListItemButton>
            </ListItem>
          </Link>
        ))}
      </List>
      <Divider />
    </Box>
  );
};

export default NavigationLinks;
