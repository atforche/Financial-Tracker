import {
  type NavigationContext,
  objectToSearchParams,
} from "@/framework/navigation/navigationContext";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import { CreateFundNavigationContext } from "@/funds/create/createFundNavigationContext";
import type { FundSortOrder } from "@/funds/types";
import type { Route } from "next";

/**
 * Search parameters for viewing the collection of funds.
 */
interface FundIndexSearchParams {
  search?: string;
  sort?: FundSortOrder;
  page?: number;
}

/**
 * Navigation context for viewing the collection of funds.
 */
class FundIndexNavigationContext implements NavigationContext {
  private readonly searchParams: FundIndexSearchParams;

  /**
   * Creates a navigation context for the supplied search parameters.
   */
  public constructor(searchParams: FundIndexSearchParams) {
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
    return `/funds?${objectToSearchParams(this.searchParams).toString()}` as Route;
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
        label: "Funds",
        href: this.getRoute(),
      },
    ];
  }

  /**
   * Gets the route for creating a fund.
   */
  public toCreateFund(): Route {
    return new CreateFundNavigationContext({}).getRoute();
  }

  /**
   * Gets the route for viewing the details of a fund.
   */
  public toFundDetail(fundId: string): Route {
    return;
  }
}

export type { FundIndexSearchParams };
export default FundIndexNavigationContext;
