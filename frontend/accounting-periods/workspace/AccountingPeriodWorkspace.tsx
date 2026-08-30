import {
  AccountingPeriodSort,
  AccountingPeriodWithBalanceSort,
} from "@/accounting-periods/types";
import {
  compactSearchParams,
  normalizeIntegerSearchParams,
  toRepeatedSearchParams,
} from "@/framework/routes/helpers";
import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import type { AccountingPeriodWorkspaceAction } from "@/accounting-periods/workspace/helpers";
import AccountingPeriodWorkspaceActions from "@/accounting-periods/workspace/AccountingPeriodWorkspaceActions";
import AccountingPeriodWorkspaceFilter from "@/accounting-periods/workspace/AccountingPeriodWorkspaceFilter";
import AccountingPeriodWorkspaceListFrame from "@/accounting-periods/workspace/AccountingPeriodWorkspaceListFrame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters supported by the accounting period workspace.
 */
interface AccountingPeriodWorkspaceSearchParams {
  years?: number | number[];
  months?: number | number[];
  sort?: AccountingPeriodWithBalanceSort;
  page?: number | string | null;
  pageSize?: number | string | null;
  fundGoalPage?: number | string | null;
  accountGoalPage?: number | string | null;
  transactionPage?: number | string | null;
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
  const apiClient = await createApiClient();
  const { years, months, sort, page, pageSize, action } = await searchParams;
  const currentPage = normalizePageValue(page);
  const rowsPerPage = getRowsPerPage(pageSize);
  const currentYear = new Date().getFullYear();
  const normalizedMonths = normalizeIntegerSearchParams(
    toRepeatedSearchParams(months),
    1,
    12,
  );

  const firstAccountingPeriodResponse = await apiClient.GET(
    "/accounting-periods",
    {
      params: {
        query: {
          Sort: AccountingPeriodSort.Date,
          Limit: 1,
        },
      },
    },
  );
  const latestAccountingPeriodResponse = await apiClient.GET(
    "/accounting-periods/with-balances",
    {
      params: {
        query: {
          Sort: AccountingPeriodWithBalanceSort.DateDescending,
          Limit: 1,
        },
      },
    },
  );
  const firstAccountingPeriod = unwrapApiResponse(
    firstAccountingPeriodResponse,
    "Failed to fetch the first accounting period",
  );
  const latestAccountingPeriod =
    unwrapApiResponse(
      latestAccountingPeriodResponse,
      "Failed to fetch the latest accounting period",
    ).items[0] ?? null;
  const firstAccountingPeriodYear =
    firstAccountingPeriod.items[0]?.year ?? currentYear;
  const normalizedYears = normalizeIntegerSearchParams(
    toRepeatedSearchParams(years),
    firstAccountingPeriodYear,
    currentYear,
  );

  const accountingPeriodsResponse = await apiClient.GET(
    "/accounting-periods/with-balances",
    {
      params: {
        query: compactSearchParams({
          "Filter.Years": normalizedYears,
          "Filter.Months": normalizedMonths,
          Sort: sort ?? null,
          Limit: rowsPerPage,
          Offset: getPageOffset(currentPage, rowsPerPage),
        }),
      },
    },
  );

  const accountingPeriods = unwrapApiResponse(
    accountingPeriodsResponse,
    "Failed to fetch accounting periods",
  );

  const isInOnboardingMode = firstAccountingPeriod.items.length === 0;

  return (
    <PageLayout>
      <AccountingPeriodWorkspaceFilter
        firstAccountingPeriod={firstAccountingPeriod.items[0] ?? null}
        isInOnboardingMode={isInOnboardingMode}
      />
      <AccountingPeriodWorkspaceListFrame
        data={accountingPeriods.items}
        totalCount={accountingPeriods.totalCount}
      />
      <AccountingPeriodWorkspaceActions
        isInOnboardingMode={isInOnboardingMode}
        latestAccountingPeriod={latestAccountingPeriod}
        selectedAccountingPeriod={null}
        requestedAction={action ?? null}
      />
    </PageLayout>
  );
};

export type { AccountingPeriodWorkspaceSearchParams };
export default AccountingPeriodWorkspace;
