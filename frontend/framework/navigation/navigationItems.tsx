import {
  AccountBalance,
  Assessment,
  CalendarMonth,
  EmojiEvents,
  GridView,
  ReceiptLong,
  Timeline,
  Workspaces,
} from "@mui/icons-material";
import type { NavigationLink } from "./navigationLinkTypes";
import accountRoutes from "@/accounts/routes";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import fundGoalRoutes from "@/fund-goals/routes";
import fundRoutes from "@/funds/routes";
import transactionRoutes from "@/transactions/routes";

/**
 * Navigation items for the application shell.
 */
const navigationItems: NavigationLink[] = [
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
    name: "Goals",
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
    name: "Transactions",
    href: transactionRoutes.workspace({}),
    icon: <ReceiptLong />,
    childLinks: [
      {
        name: "Workspace",
        href: transactionRoutes.workspace({}),
        icon: <Workspaces />,
      },
      {
        name: "Trends",
        href: transactionRoutes.trends({}),
        icon: <Timeline />,
      },
    ],
  },
];

export default navigationItems;
