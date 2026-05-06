import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Route } from "next";
import { buildAccountingPeriodDetailNavigationContext } from "@/accounting-periods/detail/accountingPeriodDetailNavigationContext";

/**
 * Parameters for closing an accounting period.
 */
interface CloseAccountingPeriodViewParams {
  id: string;
}

/**
 * Interface representing the navigation context for closing an accounting period.
 */
interface CloseAccountingPeriodNavigationContext {
  readonly route: Route;
  readonly redirect: Route;
  readonly breadcrumbs: Breadcrumb[];
  readonly routeAccountingPeriod: AccountingPeriod;
}

/**
 * Builds the route for closing an accounting period.
 */
const buildCloseAccountingPeriodRoute = function (
  params: CloseAccountingPeriodViewParams,
): Route {
  return `/accounting-periods/${params.id}/close` as Route;
};

/**
 * Builds the navigation context for closing an accounting period.
 */
const buildCloseAccountingPeriodNavigationContext = async function (
  params: CloseAccountingPeriodViewParams,
): Promise<CloseAccountingPeriodNavigationContext> {
  const previousNavigationContext =
    await buildAccountingPeriodDetailNavigationContext(
      {
        id: params.id,
      },
      {},
    );
  return {
    route: buildCloseAccountingPeriodRoute(params),
    redirect: previousNavigationContext.route,
    breadcrumbs: [
      ...previousNavigationContext.breadcrumbs,
      {
        label: "Close",
        href: buildCloseAccountingPeriodRoute(params),
      },
    ],
    routeAccountingPeriod: previousNavigationContext.routeAccountingPeriod,
  };
};

export {
  type CloseAccountingPeriodViewParams,
  type CloseAccountingPeriodNavigationContext,
  buildCloseAccountingPeriodRoute,
  buildCloseAccountingPeriodNavigationContext,
};
