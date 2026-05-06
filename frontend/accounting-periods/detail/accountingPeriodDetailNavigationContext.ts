import { type NavigationContext, objectToSearchParams } from "@/framework/navigation/navigationContext";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodIndexNavigationContext from "@/accounting-periods/index/accountingPeriodIndexNavigationContext";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Route } from "next";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for viewing an accounting period.
 */
interface AccountingPeriodDetailParams {
  id: string;
}

/**
 * Navigation context for viewing an accounting period.
 */
class AccountingPeriodDetailNavigationContext implements NavigationContext {
  private readonly params: AccountingPeriodDetailParams;
  private readonly previousNavigationContext: AccountingPeriodIndexNavigationContext;
  private routeAccountingPeriod: AccountingPeriod | null = null;

  /**
   * Creates a navigation context for the supplied parameters.
   */
  public constructor(params: AccountingPeriodDetailParams) {
    this.params = params;
    this.previousNavigationContext = new AccountingPeriodIndexNavigationContext({});
  }

  /**
   * Populates the navigation context with data from the API.
   */
  public async populate(): Promise<void> {
    const apiClient = getApiClient();
    const accountingPeriodPromise = apiClient.GET(`/accounting-periods/{accountingPeriodId}`, {
      params: {
        path: {
          accountingPeriodId: this.params.id,
        },
      }
    });
    return accountingPeriodPromise.then(({ data }) => {
      this.routeAccountingPeriod = data ?? null;
    });
  }

  /**
   * Gets the route for viewing the details of an accounting period.
   */
  public getRoute(): Route {
    return `/accounting-periods/${this.params.id}` as Route;
  }

  /**
   * Gets the redirect target for the current navigation context.
   */
  public getRedirect(): Route {
    return this.previousNavigationContext.getRoute();
  }

  /**
   * Gets the breadcrumbs for the current navigation context.
   */
  public getBreadcrumbs(): Breadcrumb[] {
    return [
      ...this.previousNavigationContext.getBreadcrumbs(),
      {
        label: this.getRouteAccountingPeriod().name,
        href: this.getRoute(),
      },
    ];
  }

  /**
   * Gets the route Accounting Period for this navigation context.
   */
  public getRouteAccountingPeriod(): AccountingPeriod {
    if (!this.routeAccountingPeriod) {
      throw new Error("Accounting period data has not been loaded yet.");
    }
    return this.routeAccountingPeriod;
  }
};

export type { AccountingPeriodDetailParams };
export default AccountingPeriodDetailNavigationContext;