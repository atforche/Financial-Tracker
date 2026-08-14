import { summarizeAccounts, summarizeFunds } from "@/overview/helpers";
import AccountOverview from "@/overview/AccountOverview";
import AccountingPeriodOverview from "@/overview/AccountingPeriodOverview";
import { AccountingPeriodSortModel } from "@/framework/data/api";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import ContentSurface from "@/framework/view/ContentSurface";
import FundOverview from "@/overview/FundOverview";
import type { JSX } from "react";
import type { OverviewData } from "@/overview/types";
import OverviewPageHeader from "@/overview/OverviewPageHeader";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
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
 * Component that displays the Overview view.
 */
const OverviewView = async function (): Promise<JSX.Element> {
  const data = await getOverviewData();

  return (
    <ConstrainedContent>
      <PageLayout>
        <ContentSurface>
          <OverviewPageHeader
            title={`Overview${
              data.currentAccountingPeriod === null
                ? ""
                : ` (${data.currentAccountingPeriod.name})`
            }`}
          />
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
      </PageLayout>
    </ConstrainedContent>
  );
};

export default OverviewView;
