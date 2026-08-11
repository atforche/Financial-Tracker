import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import { summarizeAccounts, summarizeFunds } from "@/overview/helpers";
import AccountOverview from "@/overview/AccountOverview";
import AccountingPeriodOverview from "@/overview/AccountingPeriodOverview";
import { AccountingPeriodSortModel } from "@/framework/data/api";
import type { AccountingPeriodWithTransactions } from "@/accounting-periods/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import ContentSurface from "@/framework/view/ContentSurface";
import CurrentTransactionListFrame from "@/overview/CurrentTransactionListFrame";
import FundOverview from "@/overview/FundOverview";
import type { JSX } from "react";
import type { OverviewData } from "@/overview/types";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import type { TransactionSort } from "@/transactions/types";
import { Typography } from "@mui/material";
import createApiClient from "@/framework/data/createApiClient";
import loadAllPages from "@/framework/data/loadAllPages";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Loads all data required by the overview page.
 */
const getOverviewData = async function (): Promise<OverviewData> {
  const apiClient = await createApiClient();
  const accountSummaryPromise = apiClient.GET("/accounts/with-balances");
  const fundSummaryPromise = apiClient.GET("/funds/with-balances");
  const accountingPeriodsPromise = loadAllPages(async (limit, offset) =>
    unwrapApiResponse(
      await apiClient.GET("/accounting-periods", {
        params: {
          query: {
            Sort: AccountingPeriodSortModel.DateDescending,
            Limit: limit,
            Offset: offset,
          },
        },
      }),
      "Failed to fetch accounting periods",
    ),
  );

  const [accountSummaryResponse, fundSummaryResponse, accountingPeriods] =
    await Promise.all([
      accountSummaryPromise,
      fundSummaryPromise,
      accountingPeriodsPromise,
    ]);

  const accounts = unwrapApiResponse(
    accountSummaryResponse,
    "Failed to fetch account summary",
  );
  const funds = unwrapApiResponse(
    fundSummaryResponse,
    "Failed to fetch fund summary",
  );

  return {
    accountSummary: summarizeAccounts(accounts.items),
    fundSummary: summarizeFunds(funds.items),
    latestAccountingPeriod: accountingPeriods[0] ?? null,
    currentAccountingPeriod:
      accountingPeriods.find((period) => period.isOpen) ?? null,
  };
};

/**
 * Search parameters supported by the overview page.
 */
interface OverviewSearchParams {
  currentTransactionSort?: TransactionSort;
  currentTransactionPage?: number | string | null;
  pageSize?: number | string | null;
}

/**
 * Props for the OverviewView component.
 */
interface OverviewViewProps {
  readonly searchParams: Promise<OverviewSearchParams>;
}

/**
 * Component that displays the Overview view.
 */
const OverviewView = async function ({
  searchParams,
}: OverviewViewProps): Promise<JSX.Element> {
  const { currentTransactionPage, currentTransactionSort, pageSize } =
    await searchParams;
  const data = await getOverviewData();
  const currentPage = normalizePageValue(currentTransactionPage);
  const rowsPerPage = getRowsPerPage(pageSize);
  const apiClient = await createApiClient();
  const currentTransactions: AccountingPeriodWithTransactions | null =
    data.currentAccountingPeriod === null
      ? null
      : unwrapApiResponse(
          await apiClient.GET(
            "/accounting-periods/{accountingPeriodId}/transactions",
            {
              params: {
                path: { accountingPeriodId: data.currentAccountingPeriod.id },
                query: {
                  ...(currentTransactionSort === undefined
                    ? {}
                    : { Sort: currentTransactionSort }),
                  Limit: rowsPerPage,
                  Offset: getPageOffset(currentPage, rowsPerPage),
                },
              },
            },
          ),
          "Failed to fetch current accounting period transactions",
        );

  return (
    <ConstrainedContent>
      <PageLayout>
        <ContentSurface>
          <Typography variant="h4">
            Overview
            {data.currentAccountingPeriod === null
              ? ""
              : ` (${data.currentAccountingPeriod.name})`}
          </Typography>
        </ContentSurface>

        <ResponsiveGrid columns={{ xs: 1 }}>
          <AccountingPeriodOverview
            latestAccountingPeriod={data.latestAccountingPeriod}
          />
          <ResponsiveGrid columns={{ xs: 1, md: 2 }}>
            <AccountOverview summary={data.accountSummary} />
            <FundOverview summary={data.fundSummary} />
          </ResponsiveGrid>
        </ResponsiveGrid>
        <CurrentTransactionListFrame accountingPeriod={currentTransactions} />
      </PageLayout>
    </ConstrainedContent>
  );
};

export type { OverviewSearchParams };
export default OverviewView;
