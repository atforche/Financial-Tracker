import { type NavigationContext, objectToSearchParams } from "@/framework/navigation/navigationContext";
import AccountingPeriodDetailNavigationContext from "@/accounting-periods/detail/accountingPeriodDetailNavigationContext";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import FundIndexNavigationContext from "@/funds/index/fundIndexNavigationContext";
import type { Route } from "next";

/**
 * Search parameters for creating a fund.
 */
interface CreateFundViewSearchParams {
  accountingPeriodId?: string | null;
}

/**
 * Navigation context for creating a fund.
 */
class CreateFundNavigationContext implements NavigationContext {
  private readonly searchParams: CreateFundViewSearchParams;
  private readonly previousNavigationContext: AccountingPeriodDetailNavigationContext | FundIndexNavigationContext;

  /**
   * Creates a navigation context for the supplied search parameters.
   */
  public constructor(searchParams: CreateFundViewSearchParams) {
    this.searchParams = searchParams;
    this.previousNavigationContext = typeof searchParams.accountingPeriodId === "string"
      ? new AccountingPeriodDetailNavigationContext({ id: searchParams.accountingPeriodId })
      : new FundIndexNavigationContext({});
  }

  /**
   * Populates the navigation context with data from the API.
   */
  public async populate(): Promise<void> {
    if ("populate" in this.previousNavigationContext) {
      return this.previousNavigationContext.populate();
    }
  }

  /**
   * Gets the route for the current navigation context.
   */
  public getRoute(): Route {
    return `/funds/create?${objectToSearchParams(this.searchParams).toString()}` as Route;
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
        label: "Create",
        href: this.getRoute(),
      },
    ];
  }
}

export { CreateFundNavigationContext };
export type { CreateFundViewSearchParams };