import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Route } from "next";
import { buildAccountingPeriodDetailNavigationContext } from "@/accounting-periods/detail/accountingPeriodDetailNavigationContext";

/**
 * Parameters for deleting an accounting period.
 */
interface DeleteAccountingPeriodParams {
  id: string;
}

/**
 * Interface representing the navigation context for deleting an accounting period.
 */
interface DeleteAccountingPeriodNavigationContext {
  readonly route: Route;
  readonly redirect: Route;
  readonly breadcrumbs: Breadcrumb[];
  readonly routeAccountingPeriod: AccountingPeriod;
}

/**
 * Builds the route for deleting an accounting period.
 */
const buildDeleteAccountingPeriodRoute = function (
  params: DeleteAccountingPeriodParams,
): Route {
  return `/accounting-periods/${params.id}/delete` as Route;
};

/**
 * Builds the navigation context for deleting an accounting period.
 */
const buildDeleteAccountingPeriodNavigationContext = async function (
  params: DeleteAccountingPeriodParams,
): Promise<DeleteAccountingPeriodNavigationContext> {
  const previousNavigationContext = await buildAccountingPeriodDetailNavigationContext({id: params.id}, {});
  return {
    route: `/accounting-periods/${params.id}/delete` as Route,
    redirect: previousNavigationContext.route,
    breadcrumbs: [
      ...previousNavigationContext.breadcrumbs,
      {
        label: "Delete",
        href: `/accounting-periods/${params.id}/delete` as Route,
      },
    ],
    routeAccountingPeriod: previousNavigationContext.routeAccountingPeriod,
  };
};

export {
  type DeleteAccountingPeriodParams,
  type DeleteAccountingPeriodNavigationContext,
  buildDeleteAccountingPeriodRoute,
  buildDeleteAccountingPeriodNavigationContext,
}