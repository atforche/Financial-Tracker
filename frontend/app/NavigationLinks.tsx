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
import type { Route } from "next";
import accountRoutes from "@/accounts/routes";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import fundRoutes from "@/funds/routes";
import { usePathname } from "next/navigation";

/**
 * Interface representing a navigation link in the application.
 */
interface NavigationLink {
  name: string;
  href: Route;
  icon: JSX.Element;
}

/**
 * Collection of links to be displayed in the application's navigation menu.
 */
const links: NavigationLink[] = [
  { name: "Overview", href: "/", icon: <GridView /> },
  {
    name: "Accounting Periods",
    href: accountingPeriodRoutes.index({}),
    icon: <CalendarMonth />,
  },
  { name: "Accounts", href: accountRoutes.index({}), icon: <AccountBalance /> },
  { name: "Funds", href: fundRoutes.index({}), icon: <Assessment /> },
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
