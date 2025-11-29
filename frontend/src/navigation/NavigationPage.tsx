import { Assessment, CalendarMonth, GridView } from "@mui/icons-material";
import AccountingPeriodListFrame from "@accounting-periods/AccountingPeriodListFrame";
import FundListFrame from "@funds/FundListFrame";

/**
 * Collection of all navigation pages available in the application.
 */
const NavigationPages = ["Overview", "Accounting Periods", "Funds"] as const;

/**
 * Collection of icons corresponding to each NavigationPage.
 */
const NavigationIcons = {
  Overview: <GridView key="Overview" />,
  "Accounting Periods": <CalendarMonth key="Accounting Periods" />,
  Funds: <Assessment key="Funds" />,
};

/**
 * Collection of content components corresponding to each NavigationPage.
 */
const NavigationContent = {
  Overview: null,
  "Accounting Periods": <AccountingPeriodListFrame />,
  Funds: <FundListFrame />,
};

/**
 * NavigationPage type that represents the different pages that can be navigated to.
 */
type NavigationPage = (typeof NavigationPages)[number];

export {
  NavigationContent,
  NavigationPages,
  NavigationIcons,
  type NavigationPage,
};
