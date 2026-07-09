"use client";

import {
  AccountBalance,
  Assessment,
  CalendarMonth,
  EmojiEvents,
  GridView,
  ReceiptLong,
  Timeline,
  Today,
  Workspaces,
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
  childLinks?: NavigationChildLink[];
}

interface NavigationChildLink {
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
    href: accountingPeriodRoutes.current({}),
    icon: <CalendarMonth />,
    childLinks: [
      {
        name: "Current",
        href: accountingPeriodRoutes.current({}),
        icon: <Today />,
      },
      {
        name: "Trends",
        href: accountingPeriodRoutes.trends({}),
        icon: <Timeline />,
      },
      {
        name: "Workspace",
        href: accountingPeriodRoutes.workspace({}),
        icon: <Workspaces />,
      },
    ],
  },
  {
    name: "Accounts",
    href: accountRoutes.workspace({}),
    icon: <AccountBalance />,
    childLinks: [
      {
        name: "Workspace",
        href: accountRoutes.workspace({}),
        icon: <Workspaces />,
      },
      {
        name: "Trends",
        href: accountRoutes.trends({}),
        icon: <Timeline />,
      },
    ],
  },
  {
    name: "Funds",
    href: fundRoutes.workspace({}),
    icon: <Assessment />,
    childLinks: [
      {
        name: "Workspace",
        href: fundRoutes.workspace({}),
        icon: <Workspaces />,
      },
      {
        name: "Trends",
        href: fundRoutes.trends({}),
        icon: <Timeline />,
      },
    ],
  },
  {
    name: "Goals",
    href: goalRoutes.current(),
    icon: <EmojiEvents />,
    childLinks: [
      {
        name: "Current",
        href: goalRoutes.current(),
        icon: <Today />,
      },
      {
        name: "Trends",
        href: goalRoutes.trends({}),
        icon: <Timeline />,
      },
      {
        name: "Workspace",
        href: goalRoutes.workspace({}),
        icon: <Workspaces />,
      },
    ],
  },
  {
    name: "Transactions",
    href: transactionRoutes.current(),
    icon: <ReceiptLong />,
    childLinks: [
      {
        name: "Current",
        href: transactionRoutes.current(),
        icon: <Today />,
      },
      {
        name: "Trends",
        href: transactionRoutes.trends({}),
        icon: <Timeline />,
      },
      {
        name: "Workspace",
        href: transactionRoutes.workspace({}),
        icon: <Workspaces />,
      },
    ],
  },
];

/**
 * Components displaying the navigation links for the application.
 */
const NavigationLinks = function (): JSX.Element {
  const pathname = usePathname();
  const [expandedLinkName, setExpandedLinkName] = useState<string | null>(null);

  // Determine which link should be expanded based on current pathname
  useEffect(() => {
    const expandedLink = links.find(
      (link) =>
        typeof link.childLinks !== "undefined" &&
        (pathname === link.href ||
          link.childLinks.some((child) => pathname === child.href)),
    );
    setExpandedLinkName(expandedLink?.name ?? null);
  }, [pathname]);

  return (
    <Box sx={{ overflow: "auto" }}>
      <Divider />
      <List>
        {links.map((link) => {
          if (!link.childLinks) {
            return (
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
            );
          }

          const isExpanded = expandedLinkName === link.name;
          const isSelected =
            pathname === link.href ||
            link.childLinks.some((child) => pathname === child.href);
          return (
            <div key={link.name}>
              <ListItem
                disablePadding
                secondaryAction={
                  <IconButton
                    edge="end"
                    aria-label={
                      isExpanded
                        ? `Collapse ${link.name}`
                        : `Expand ${link.name}`
                    }
                    onClick={() => {
                      setExpandedLinkName(isExpanded ? null : link.name);
                    }}
                  >
                    {isExpanded ? <ExpandLess /> : <ExpandMore />}
                  </IconButton>
                }
              >
                <Link
                  href={link.href}
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
              </ListItem>
              <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                <List disablePadding>
                  {link.childLinks.map((childLink) => (
                    <Link
                      key={childLink.name}
                      href={childLink.href}
                      style={{ textDecoration: "none", color: "inherit" }}
                    >
                      <ListItem disablePadding>
                        <ListItemButton
                          selected={pathname === childLink.href}
                          sx={{ pl: 7.5 }}
                        >
                          <ListItemIcon sx={{ minWidth: 36 }}>
                            {childLink.icon}
                          </ListItemIcon>
                          <ListItemText primary={childLink.name} />
                        </ListItemButton>
                      </ListItem>
                    </Link>
                  ))}
                </List>
              </Collapse>
            </div>
          );
        })}
      </List>
      <Divider />
    </Box>
  );
};

export default NavigationLinks;
