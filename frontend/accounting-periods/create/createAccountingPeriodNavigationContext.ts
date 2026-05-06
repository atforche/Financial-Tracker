import AccountingPeriodIndexNavigationContext from "@/accounting-periods/index/accountingPeriodIndexNavigationContext";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { NavigationContext } from "@/framework/navigation/navigationContext";
import type { Route } from "next";

/**
 * Navigation context for creating an accounting period.
 */
class CreateAccountingPeriodNavigationContext implements NavigationContext {
  private readonly previousNavigationContext: AccountingPeriodIndexNavigationContext = new AccountingPeriodIndexNavigationContext({});

  /**
   * Populates the navigation context with data from the API.
   */
  public async populate(): Promise<void> {
    return Promise.resolve();
  }

  /**
   * Gets the route for creating an accounting period.
   */
  public getRoute(): Route {
    return "/accounting-periods/create" as Route;
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
        label: "Create Accounting Period",
        href: this.getRoute(),
      },
    ];
  }
};

export default CreateAccountingPeriodNavigationContext;