"use client";

import type { AccountingPeriodDetailView } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import type { JSX } from "react";
import type { Route } from "next";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import { useRouter } from "next/navigation";

interface AccountingPeriodDetailNavigationProps {
  readonly view: AccountingPeriodDetailView;
  readonly progressHref: Route;
  readonly planHref: Route;
}

/** Displays navigation between accounting-period progress and planning. */
const AccountingPeriodDetailNavigation = function ({
  view,
  progressHref,
  planHref,
}: AccountingPeriodDetailNavigationProps): JSX.Element {
  const router = useRouter();

  return (
    <ToggleButtonSelector
      value={view}
      options={[
        { value: "progress", label: "Progress" },
        { value: "plan", label: "Plan" },
      ]}
      onChange={(nextView) => {
        router.push(nextView === "progress" ? progressHref : planHref);
      }}
    />
  );
};

export default AccountingPeriodDetailNavigation;
