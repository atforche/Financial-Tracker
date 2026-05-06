import type {
  AccountingPeriod,
  AccountingPeriodAccountSortOrder,
  AccountingPeriodFundSortOrder,
  AccountingPeriodGoalSortOrder,
  AccountingPeriodTransactionSortOrder,
} from "@/accounting-periods/types";
import type { Breadcrumb } from "@/framework/Breadcrumbs";
import type { Route } from "next";
import type { ToggleState } from "@/accounting-periods/detail/AccountingPeriodDetailViewListFrames";
import { buildAccountingPeriodIndexNavigationContext } from "@/accounting-periods/index/accountingPeriodIndexNavigationContext";
import getApiClient from "@/framework/data/getApiClient";
import { objectToSearchParams } from "@/framework/navigation/navigationContext";

/**
 * Parameters for viewing an accounting period.
 */
interface AccountingPeriodDetailParams {
  id: string;
}

/**
 * Search parameters for viewing an accounting period.
 */
interface AccountingPeriodDetailSearchParams {
  search?: string;
  fundSort?: AccountingPeriodFundSortOrder;
  goalSort?: AccountingPeriodGoalSortOrder;
  accountSort?: AccountingPeriodAccountSortOrder;
  transactionSort?: AccountingPeriodTransactionSortOrder;
  page?: number;
  display?: ToggleState;
}

/**
 * Interface representing the navigation context for viewing an accounting period.
 */
interface AccountingPeriodDetailNavigationContext {
  readonly route: Route;
  readonly redirect: Route;
  readonly breadcrumbs: Breadcrumb[];
  readonly routeAccountingPeriod: AccountingPeriod;
}

/**
 * Builds the route for viewing an accounting period.
 */
const buildAccountingPeriodDetailRoute = function (
  params: AccountingPeriodDetailParams,
  searchParams: AccountingPeriodDetailSearchParams,
): Route {
  return `/accounting-periods/${params.id}?${objectToSearchParams(searchParams).toString()}` as Route;
};

/**
 * Builds the navigation context for viewing an accounting period.
 */
const buildAccountingPeriodDetailNavigationContext = async function (
  params: AccountingPeriodDetailParams,
  searchParams: AccountingPeriodDetailSearchParams,
): Promise<AccountingPeriodDetailNavigationContext> {
  const previousNavigationContext = buildAccountingPeriodIndexNavigationContext(
    {},
  );

  const apiClient = getApiClient();
  const accountingPeriodPromise = apiClient.GET(
    `/accounting-periods/{accountingPeriodId}`,
    {
      params: {
        path: {
          accountingPeriodId: params.id,
        },
      },
    },
  );
  return accountingPeriodPromise.then(({ data: accountingPeriod }) => {
    if (!accountingPeriod) {
      throw new Error("Accounting period data could not be loaded.");
    }
    return {
      route: buildAccountingPeriodDetailRoute(params, searchParams),
      redirect: previousNavigationContext.route,
      breadcrumbs: [
        ...previousNavigationContext.breadcrumbs,
        {
          label: accountingPeriod.name,
          href: buildAccountingPeriodDetailRoute(params, searchParams),
        },
      ],
      routeAccountingPeriod: accountingPeriod,
    };
  });
};

export {
  type AccountingPeriodDetailParams,
  type AccountingPeriodDetailSearchParams,
  type AccountingPeriodDetailNavigationContext,
  buildAccountingPeriodDetailRoute,
  buildAccountingPeriodDetailNavigationContext,
};
