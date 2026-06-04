import type { AccountingPeriodDashboardSearchParams } from "@/accounting-periods/dashboard/AccountingPeriodDashboard";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

const pathWithSearchParams = function (
  pathname: string,
  searchParams: URLSearchParams,
): Route {
  const query = searchParams.toString();
  return query === "" ? pathname : `${pathname}?${query}`;
};

/**
 * App routes related to accounting periods.
 */
const routes = {
  dashboard: (searchParams: AccountingPeriodDashboardSearchParams): Route =>
    pathWithSearchParams(
      "/accounting-periods/dashboard",
      objectToSearchParams(searchParams),
    ),
  workspace: (searchParams: AccountingPeriodWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/accounting-periods/workspace",
      objectToSearchParams(searchParams),
    ),
};

export default routes;
