import {
  AccountBalance,
  Assessment,
  CalendarMonth,
  EmojiEvents,
  Flag,
  GridView,
  Place,
  ReceiptLong,
  Timeline,
  Workspaces,
} from "@mui/icons-material";
import type { NavigationLink } from "./navigationLinkTypes";
import accountGoalRoutes from "@/account-goals/routes";
import accountRoutes from "@/accounts/routes";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import fundGoalRoutes from "@/fund-goals/routes";
import fundRoutes from "@/funds/routes";
import transactionRoutes from "@/transactions/routes";

/**
 * Navigation items for the application shell.
 */
const navigationItems = function (): NavigationLink[] {
  const items: NavigationLink[] = [
    { name: "Overview", href: "/", icon: <GridView /> },
    {
      name: "Accounting Periods",
      href: accountingPeriodRoutes.workspace({}),
      icon: <CalendarMonth />,
      childLinks: [
        {
          name: "Workspace",
          href: accountingPeriodRoutes.workspace({}),
          icon: <Workspaces />,
        },
        {
          name: "Trends",
          href: accountingPeriodRoutes.trends({}),
          icon: <Timeline />,
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
        { name: "Trends", href: accountRoutes.trends({}), icon: <Timeline /> },
      ],
    },
    {
      name: "Account Goals",
      href: accountGoalRoutes.workspace({}),
      icon: <Flag />,
      childLinks: [
        {
          name: "Workspace",
          href: accountGoalRoutes.workspace({}),
          icon: <Workspaces />,
        },
        {
          name: "Trends",
          href: accountGoalRoutes.trends({}),
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
        { name: "Trends", href: fundRoutes.trends({}), icon: <Timeline /> },
      ],
    },
    {
      name: "Fund Goals",
      href: fundGoalRoutes.workspace({}),
      icon: <EmojiEvents />,
      childLinks: [
        {
          name: "Workspace",
          href: fundGoalRoutes.workspace({}),
          icon: <Workspaces />,
        },
        { name: "Trends", href: fundGoalRoutes.trends({}), icon: <Timeline /> },
      ],
    },
    {
      name: "Locations",
      href: "/locations",
      icon: <Place />,
      childLinks: [
        { name: "Workspace", href: "/locations", icon: <Workspaces /> },
        { name: "Trends", href: "/locations/trends", icon: <Timeline /> },
      ],
    },
    {
      name: "Transactions",
      href: transactionRoutes.workspace({}),
      icon: <ReceiptLong />,
    },
  ];
  return items;
};

export default navigationItems;
