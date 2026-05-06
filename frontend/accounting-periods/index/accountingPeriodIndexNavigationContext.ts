import { type NavigationContext, objectToSearchParams } from "@/framework/navigation/navigationContext";
import AccountingPeriodDetailNavigationContext from "@/accounting-periods/detail/accountingPeriodDetailNavigationContext";
import type { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import CreateAccountingPeriodNavigationContext from "@/accounting-periods/create/createAccountingPeriodNavigationContext";
import type { Route } from "next";

/**
 * Search parameters for viewing a collection of accounting periods..
 */
interface AccountingPeriodIndexSearchParams {
  search?: string;
  sort?: AccountingPeriodSortOrder;
  page?: number;
}

/**
 * Navigation context for viewing the collection of accounting periods.
 */
class AccountingPeriodIndexNavigationContext implements NavigationContext {
  private readonly searchParams: AccountingPeriodIndexSearchParams;

  /**
   * Creates a navigation context for the supplied search parameters.
   */
  public constructor(searchParams: AccountingPeriodIndexSearchParams) {
    this.searchParams = searchParams;
  }

  /**
   * Populates the navigation context with data from the API.
   */
  public async populate(): Promise<void> {
    return Promise.resolve();
  }

  /**
   * Gets the route for the current navigation context.
   */
  public getRoute(): Route {
    return `/accounting-periods?${objectToSearchParams(this.searchParams).toString()}` as Route;
  }

  /**
   * Gets the redirect target for the current navigation context.
   */
  public getRedirect(): Route {
    return "/" as Route;
  }

  /**
   * Gets the breadcrumbs for the current navigation context.
   */
  public getBreadcrumbs(): Breadcrumb[] {
    return [
      {
        label: "Accounting Periods",
        href: this.getRoute(),
      }
    ];
  }

  /**
   * Gets the route for creating an accounting period.
   */
  public toCreateAccountingPeriod(): Route {
    return new CreateAccountingPeriodNavigationContext().getRoute();
  }

  /**
   * Gets the route for viewing the details of an accounting period.
   */
  public toAccountingPeriodDetail(accountingPeriodId: string): Route {
    return new AccountingPeriodDetailNavigationContext({ id: accountingPeriodId }).getRoute();
  }
}

export type { AccountingPeriodIndexSearchParams };
export default AccountingPeriodIndexNavigationContext;