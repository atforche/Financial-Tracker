"use client";

import {
  AccountBalance,
  Assessment,
  CalendarMonth,
  EmojiEvents,
  GridView,
  ReceiptLong,
} from "@mui/icons-material";
import {
  Box,
  Collapse,
  Divider,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from "@mui/material";
import { type JSX, useEffect, useState } from "react";
import ExpandLess from "@mui/icons-material/ExpandLess";
import ExpandMore from "@mui/icons-material/ExpandMore";
import Link from "next/link";
import type { Route } from "next";
import accountRoutes from "@/accounts/routes";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import fundRoutes from "@/funds/routes";
import goalRoutes from "@/goals/routes";
import transactionRoutes from "@/transactions/routes";
import { usePathname } from "next/navigation";

/**
 * Interface representing a navigation link in the application.
 */
interface NavigationLink {
  name: string;
  href: Route;
  icon: JSX.Element;
}

interface NavigationChildLink {
  name: string;
  href: Route;
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
  {
    name: "Transactions",
    href: transactionRoutes.index({}),
    icon: <ReceiptLong />,
  },
  {
    name: "Goals",
    href: goalRoutes.index({}),
    icon: <EmojiEvents />,
  },
  { name: "Funds", href: fundRoutes.index({}), icon: <Assessment /> },
];

const accountChildLinks: NavigationChildLink[] = [
  {
    name: "Dashboard",
    href: accountRoutes.dashboard({}),
  },
  {
    name: "Workspace",
    href: accountRoutes.workspace({}),
  },
];

/**
 * Components displaying the navigation links for the application.
 */
const NavigationLinks = function (): JSX.Element {
  const pathname = usePathname();
  const isOnAccountsRoute =
    pathname === "/accounts" ||
    pathname.startsWith("/accounts/dashboard") ||
    pathname.startsWith("/accounts/workspace");
  const [isAccountsExpanded, setIsAccountsExpanded] =
    useState<boolean>(isOnAccountsRoute);

  useEffect(() => {
    if (isOnAccountsRoute) {
      setIsAccountsExpanded(true);
      return;
    }

    setIsAccountsExpanded(false);
  }, [isOnAccountsRoute]);

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
        <ListItem
          disablePadding
          secondaryAction={
            <IconButton
              edge="end"
              aria-label={
                isAccountsExpanded ? "Collapse Accounts" : "Expand Accounts"
              }
              onClick={() => {
                setIsAccountsExpanded((currentValue) => !currentValue);
              }}
            >
              {isAccountsExpanded ? <ExpandLess /> : <ExpandMore />}
            </IconButton>
          }
        >
          <Link
            href={accountRoutes.dashboard({})}
            style={{ textDecoration: "none", color: "inherit", width: "100%" }}
          >
            <ListItemButton selected={isOnAccountsRoute}>
              <ListItemIcon sx={{ paddingLeft: "15px" }}>
                <AccountBalance />
              </ListItemIcon>
              <ListItemText primary="Accounts" />
            </ListItemButton>
          </Link>
        </ListItem>
        <Collapse in={isAccountsExpanded} timeout="auto" unmountOnExit>
          <List disablePadding>
            {accountChildLinks.map((link) => (
              <Link
                key={link.name}
                href={link.href}
                style={{ textDecoration: "none", color: "inherit" }}
              >
                <ListItem disablePadding>
                  <ListItemButton
                    selected={pathname === link.href}
                    sx={{ pl: 7.5 }}
                  >
                    <ListItemText primary={link.name} />
                  </ListItemButton>
                </ListItem>
              </Link>
            ))}
          </List>
        </Collapse>
      </List>
      <Divider />
    </Box>
  );
};

export default NavigationLinks;
