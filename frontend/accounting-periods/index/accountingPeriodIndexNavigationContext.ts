import type { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/navigation/navigationContext";

/**
 * Search parameters for viewing a collection of accounting periods..
 */
interface AccountingPeriodIndexSearchParams {
  search?: string;
  sort?: AccountingPeriodSortOrder;
  page?: number;
}

/**
 * Interface representing the navigation context for viewing the collection of accounting periods.
 */
interface AccountingPeriodIndexNavigationContext {
  readonly route: Route;
  readonly redirect: Route;
  readonly breadcrumbs: Breadcrumb[];
}

/**
 * Builds the navigation context for viewing the collection of accounting periods.
 */
const buildAccountingPeriodIndexNavigationContext = function (
  searchParams: AccountingPeriodIndexSearchParams | null,
): AccountingPeriodIndexNavigationContext {
  return {
    route:
      `/accounting-periods?${objectToSearchParams(searchParams).toString()}` as Route,
    redirect: "/" as Route,
    breadcrumbs: [
      {
        label: "Accounting Periods",
        href: `/accounting-periods?${objectToSearchParams(searchParams).toString()}` as Route,
      },
    ],
  };
};

export {
  type AccountingPeriodIndexSearchParams,
  type AccountingPeriodIndexNavigationContext,
  buildAccountingPeriodIndexNavigationContext,
};
