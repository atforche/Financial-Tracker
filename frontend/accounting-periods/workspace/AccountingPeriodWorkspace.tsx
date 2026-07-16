import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import type { AccountingPeriodWithBalanceSortValue } from "@/accounting-periods/types";
import AccountingPeriodWorkspaceActions from "@/accounting-periods/workspace/AccountingPeriodWorkspaceActions";
import AccountingPeriodWorkspaceFilter from "@/accounting-periods/workspace/AccountingPeriodWorkspaceFilter";
import AccountingPeriodWorkspaceListFrame from "@/accounting-periods/workspace/AccountingPeriodWorkspaceListFrame";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import getApiClient from "@/framework/data/getApiClient";
import getApiData from "@/framework/data/apiResponse";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

type AccountingPeriodWorkspaceAction = "create" | "close" | "reopen" | "delete";

/**
 * Search parameters supported by the Accounting Period workspace.
 */
interface AccountingPeriodWorkspaceSearchParams {
  years?: number | number[];
  months?: number | number[];
  sort?: AccountingPeriodWithBalanceSortValue;
  page?: number | string | null;
  selectedAccountingPeriodId?: string;
  action?: AccountingPeriodWorkspaceAction;
}

/**
 * Props for the AccountingPeriodWorkspace component.
 */
interface AccountingPeriodWorkspaceProps {
  readonly searchParams: Promise<AccountingPeriodWorkspaceSearchParams>;
}

/**
 * Displays the accounting period workspace.
 */
const AccountingPeriodWorkspace = async function ({
  searchParams,
}: AccountingPeriodWorkspaceProps): Promise<JSX.Element> {
  const apiClient = getApiClient();
  const { years, months, sort, page, selectedAccountingPeriodId, action } =
    await searchParams;
  const currentPage = normalizePageValue(page);

  const firstAccountingPeriodPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Sort: "Date",
        Limit: 1,
      },
    },
  });
  const accountingPeriodsPromise = apiClient.GET(
    "/accounting-periods/with-balances",
    {
      params: {
        query: {
          ...(Array.isArray(years)
            ? { "Filter.Years": years }
            : typeof years !== "undefined"
              ? { "Filter.Years": [years] }
              : {}),
          ...(Array.isArray(months)
            ? { "Filter.Months": months }
            : typeof months !== "undefined"
              ? { "Filter.Months": [months] }
              : {}),
          Sort: sort ?? null,
          Limit: rowsPerPage,
          Offset: getPageOffset(currentPage),
        },
      },
    },
  );

  const [firstAccountingPeriodResponse, accountingPeriodsResponse] =
    await Promise.all([firstAccountingPeriodPromise, accountingPeriodsPromise]);

  const firstAccountingPeriod = getApiData(
    firstAccountingPeriodResponse,
    "Failed to fetch the first accounting period",
  );
  const accountingPeriods = getApiData(
    accountingPeriodsResponse,
    "Failed to fetch accounting periods",
  );

  const selectedAccountingPeriod =
    accountingPeriods.items.find(
      (accountingPeriod) => accountingPeriod.id === selectedAccountingPeriodId,
    ) ?? null;
  const isInOnboardingMode = firstAccountingPeriod.items.length === 0;

  if (
    typeof selectedAccountingPeriodId === "string" &&
    selectedAccountingPeriod === null
  ) {
    redirect(
      routes.workspace({
        years: Array.isArray(years)
          ? years
          : typeof years !== "undefined"
            ? [years]
            : [],
        months: Array.isArray(months)
          ? months
          : typeof months !== "undefined"
            ? [months]
            : [],
        ...(typeof sort !== "undefined" ? { sort } : {}),
        ...(typeof page !== "undefined" ? { page: currentPage } : {}),
        ...(typeof action !== "undefined" ? { action } : {}),
      }),
    );
  }

  return (
    <PageLayout>
      <ConstrainedContent>
        <AccountingPeriodWorkspaceFilter
          firstAccountingPeriod={firstAccountingPeriod.items[0] ?? null}
        />
      </ConstrainedContent>
      <ResponsiveGrid minimumColumnWidth={600}>
        <AccountingPeriodWorkspaceListFrame
          data={accountingPeriods.items}
          totalCount={accountingPeriods.totalCount}
          selectedAccountingPeriodId={selectedAccountingPeriodId ?? null}
        />
        <AccountingPeriodWorkspaceActions
          isInOnboardingMode={isInOnboardingMode}
          selectedAccountingPeriod={selectedAccountingPeriod}
          requestedAction={action ?? null}
        />
      </ResponsiveGrid>
    </PageLayout>
  );
};

export type {
  AccountingPeriodWorkspaceAction,
  AccountingPeriodWorkspaceSearchParams,
};
export default AccountingPeriodWorkspace;
