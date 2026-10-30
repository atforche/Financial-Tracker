import AccountingPeriodCard from "@/accounting-periods/workspace/AccountingPeriodCard";
import AccountingPeriodProgressCard from "@/accounting-periods/workspace/AccountingPeriodProgressCard";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import AccountingPeriodTrendChart from "@/accounting-periods/trends/AccountingPeriodTrendChart";
import AccountingPeriodTrendsChangeChart from "@/accounting-periods/trends/AccountingPeriodTrendsChangeChart";
import AccountingPeriodTrendsFilter from "@/accounting-periods/trends/AccountingPeriodTrendsFilter";
import BalanceStatCards from "@/accounts/BalanceStatCards";
import CardResponsiveGrid from "@/framework/view/CardResponsiveGrid";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import SignedChange from "@/framework/view/SignedChange";
import { compareAccountingPeriods } from "@/accounting-periods/helpers";
import createApiClient from "@/framework/data/createApiClient";
import { getCurrencyDifference } from "@/framework/currencyHelpers";
import { getDefaultTrendAccountingPeriodRange } from "@/framework/routes/trendRange";
import routes from "@/accounting-periods/routes";
import { sumProgress } from "@/accounting-periods/trends/helpers";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters for the AccountingPeriodTrends component.
 */
interface AccountingPeriodTrendsSearchParams {
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
}

/**
 * Props for the AccountingPeriodTrends component.
 */
interface AccountingPeriodTrendsProps {
  readonly searchParams: Promise<AccountingPeriodTrendsSearchParams>;
}

/**
 * Displays how balances, income, Fund Goal contributions, and spending went across a range of accounting periods,
 * along with each period in the range.
 */
const AccountingPeriodTrends = async function ({
  searchParams,
}: AccountingPeriodTrendsProps): Promise<JSX.Element> {
  const { startAccountingPeriodId, endAccountingPeriodId } = await searchParams;
  const apiClient = await createApiClient();
  const accountingPeriods = unwrapApiResponse(
    await apiClient.GET("/accounting-periods", {
      params: {
        query: { Sort: AccountingPeriodSort.DateDescending, Limit: 500 },
      },
    }),
    "Failed to fetch accounting periods",
  ).items;

  // Accounting periods are sorted newest first, so the range runs from the end's index to the start's
  const defaultRange = getDefaultTrendAccountingPeriodRange(accountingPeriods);
  const start = startAccountingPeriodId ?? defaultRange?.start ?? "";
  const end = endAccountingPeriodId ?? defaultRange?.end ?? "";
  const startIndex = accountingPeriods.findIndex(({ id }) => id === start);
  const endIndex = accountingPeriods.findIndex(({ id }) => id === end);
  const selectedIds =
    startIndex < 0 || endIndex < 0
      ? []
      : accountingPeriods
          .slice(
            Math.min(startIndex, endIndex),
            Math.max(startIndex, endIndex) + 1,
          )
          .map(({ id }) => id);
  const filter = (
    <ConstrainedContent>
      <AccountingPeriodTrendsFilter
        accountingPeriods={accountingPeriods}
        disabled={accountingPeriods.length === 0}
        transactionsHref={
          selectedIds.length === 0
            ? null
            : transactionRoutes.workspace({
                accountingPeriodIds: selectedIds,
                returnUrl: routes.trends({
                  startAccountingPeriodId: start,
                  endAccountingPeriodId: end,
                }),
              })
        }
      />
    </ConstrainedContent>
  );
  if (selectedIds.length === 0) {
    return <PageLayout>{filter}</PageLayout>;
  }

  const query = { "Range.Start": start, "Range.End": end };
  const [rangeResponse, accountsResponse, fundsResponse] = await Promise.all([
    apiClient.GET("/accounting-periods/range", { params: { query } }),
    apiClient.GET("/accounts/accounting-period-range", {
      params: { query: { ...query, Limit: 1 } },
    }),
    apiClient.GET("/funds/accounting-period-range", {
      params: { query: { ...query, Limit: 1 } },
    }),
  ]);
  const range = unwrapApiResponse(
    rangeResponse,
    "Failed to fetch accounting period range",
  );
  const accountBalances = unwrapApiResponse(
    accountsResponse,
    "Failed to fetch account balance trends",
  ).accountingPeriods;
  const fundBalances = unwrapApiResponse(
    fundsResponse,
    "Failed to fetch fund balance trends",
  ).accountingPeriods;
  const periods = [...range.accountingPeriods.items].sort(
    compareAccountingPeriods,
  );
  const firstAccounts = accountBalances.at(0);
  const lastAccounts = accountBalances.at(-1);
  const firstFunds = fundBalances.at(0);
  const lastFunds = fundBalances.at(-1);

  return (
    <PageLayout>
      {filter}
      {firstAccounts === undefined ||
      lastAccounts === undefined ||
      firstFunds === undefined ||
      lastFunds === undefined ? null : (
        <BalanceStatCards
          openingBalance={firstAccounts.openingBalance}
          closingBalance={lastAccounts.closingBalance}
          unassignedBalance={lastFunds.closingBalance.totalUnassignedBalance}
          unassignedCaption={
            <>
              <SignedChange
                amount={getCurrencyDifference(
                  lastFunds.closingBalance.totalUnassignedBalance,
                  firstFunds.openingBalance.totalUnassignedBalance,
                )}
              />{" "}
              over this range
            </>
          }
          changeDescription="over this range"
        />
      )}
      <AccountingPeriodProgressCard
        accountingPeriod={sumProgress(periods)}
        totalSpending={range.totalSpending}
      />
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
        <AccountingPeriodTrendChart accountingPeriods={periods} />
        <AccountingPeriodTrendsChangeChart accountingPeriods={periods} />
      </ResponsiveGrid>
      <CardResponsiveGrid minimumColumnWidth={280} spacing={2}>
        {periods.toReversed().map((period) => (
          <AccountingPeriodCard key={period.id} period={period} />
        ))}
      </CardResponsiveGrid>
    </PageLayout>
  );
};

export type { AccountingPeriodTrendsSearchParams };
export default AccountingPeriodTrends;
