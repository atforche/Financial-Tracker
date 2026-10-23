"use client";

import { Tab, Tabs } from "@mui/material";
import type { JSX } from "react";
import Link from "next/link";
import type { Route } from "next";

/**
 * Views of an accounting period, each on its own page.
 */
type AccountingPeriodView = "cash-flow" | "plan";

/**
 * Props for the AccountingPeriodViewTabs component.
 */
interface AccountingPeriodViewTabsProps {
  readonly activeView: AccountingPeriodView;
  readonly cashFlowHref: Route;
  readonly planHref: Route;
}

/**
 * Switches between the views of an accounting period. Each view is a separate page, so the tabs are links
 * and only the selected view's data is loaded.
 */
const AccountingPeriodViewTabs = function ({
  activeView,
  cashFlowHref,
  planHref,
}: AccountingPeriodViewTabsProps): JSX.Element {
  return (
    <Tabs
      value={activeView}
      aria-label="Accounting period views"
      variant="scrollable"
      scrollButtons="auto"
    >
      <Tab
        value="cash-flow"
        label="Cash Flow"
        component={Link}
        href={cashFlowHref}
      />
      <Tab value="plan" label="Plan" component={Link} href={planHref} />
    </Tabs>
  );
};

export type { AccountingPeriodView };
export default AccountingPeriodViewTabs;
