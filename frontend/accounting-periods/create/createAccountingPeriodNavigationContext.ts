import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Route } from "next";
import { buildAccountingPeriodIndexNavigationContext } from "@/accounting-periods/index/accountingPeriodIndexNavigationContext";

/**
 * Interface representing the navigation context for creating an accounting period.
 */
interface CreateAccountingPeriodNavigationContext {
  readonly route: Route;
  readonly redirect: Route;
  readonly breadcrumbs: Breadcrumb[];
}

/**
 * Builds the route for creating an accounting period.
 */
const buildCreateAccountingPeriodRoute = function (): Route {
  return "/accounting-periods/create" as Route;
};

/**
 * Builds the navigation context for creating an accounting period.
 */
const buildCreateAccountingPeriodNavigationContext =
  function (): CreateAccountingPeriodNavigationContext {
    const previousNavigationContext =
      buildAccountingPeriodIndexNavigationContext({});

    return {
      route: "/accounting-periods/create" as Route,
      redirect: previousNavigationContext.route,
      breadcrumbs: [
        ...previousNavigationContext.breadcrumbs,
        {
          label: "Create",
          href: "/accounting-periods/create" as Route,
        },
      ],
    };
  };

export {
  type CreateAccountingPeriodNavigationContext,
  buildCreateAccountingPeriodRoute,
  buildCreateAccountingPeriodNavigationContext,
};
