import {
  AccountingPeriodSort,
  type AccountingPeriodWithBalanceSort,
} from "@/accounting-periods/types";
import {
  compactSearchParams,
  normalizeIntegerSearchParams,
  toRepeatedSearchParams,
} from "@/framework/routes/helpers";
import {
  getPageOffset,
  normalizePageValue,
  rowsPerPage,
} from "@/framework/listframe/page";
import type { AccountingPeriodWorkspaceAction } from "@/accounting-periods/workspace/helpers";
import AccountingPeriodWorkspaceActions from "@/accounting-periods/workspace/AccountingPeriodWorkspaceActions";
import AccountingPeriodWorkspaceFilter from "@/accounting-periods/workspace/AccountingPeriodWorkspaceFilter";
import AccountingPeriodWorkspaceListFrame from "@/accounting-periods/workspace/AccountingPeriodWorkspaceListFrame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters supported by the accounting period workspace.
 */
interface AccountingPeriodWorkspaceSearchParams {
  years?: number | number[];
  months?: number | number[];
  sort?: AccountingPeriodWithBalanceSort;
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
  const apiClient = await createApiClient();
  const { years, months, sort, page, selectedAccountingPeriodId, action } =
    await searchParams;
  const currentPage = normalizePageValue(page);
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
    "/accounting-periods",
    {
      params: {
        query: {
          Sort: AccountingPeriodSort.DateDescending,
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
          Offset: getPageOffset(currentPage),
        }),
      },
    },
  );

  const accountingPeriods = unwrapApiResponse(
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
      routes.workspace(
        compactSearchParams({
          years: normalizedYears,
          months: normalizedMonths,
          sort,
          page: currentPage,
          action,
        }),
      ),
    );
  }

  return (
    <PageLayout>
      <AccountingPeriodWorkspaceFilter
        firstAccountingPeriod={firstAccountingPeriod.items[0] ?? null}
        isInOnboardingMode={isInOnboardingMode}
        selectedAccountingPeriod={selectedAccountingPeriod}
      />
      <AccountingPeriodWorkspaceListFrame
        data={accountingPeriods.items}
        totalCount={accountingPeriods.totalCount}
        selectedAccountingPeriodId={selectedAccountingPeriodId ?? null}
      />
      <AccountingPeriodWorkspaceActions
        isInOnboardingMode={isInOnboardingMode}
        latestAccountingPeriod={latestAccountingPeriod}
        selectedAccountingPeriod={selectedAccountingPeriod}
        requestedAction={action ?? null}
      />
    </PageLayout>
  );
};

export type { AccountingPeriodWorkspaceSearchParams };
export default AccountingPeriodWorkspace;
