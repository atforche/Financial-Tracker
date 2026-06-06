import { Box, Paper, Stack, Typography } from "@mui/material";
import AccountOverview from "@/overview/AccountOverview";
import AccountingPeriodOverview from "@/overview/AccountingPeriodOverview";
import FundOverview from "@/overview/FundOverview";
import GoalOverview from "@/overview/GoalOverview";
import type { JSX } from "react";
import type { OverviewData } from "@/overview/types";
import TransactionOverview from "@/overview/TransactionOverview";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Loads all data required by the overview page.
 */
const getOverviewData = async function (
  searchParams: Promise<{ page?: string | string[] }>,
): Promise<OverviewData> {
  const apiClient = getApiClient();
  const resolvedSearchParams = await searchParams;
  const requestedPage = Number.parseInt(
    Array.isArray(resolvedSearchParams.page)
      ? (resolvedSearchParams.page[0] ?? "1")
      : (resolvedSearchParams.page ?? "1"),
    10,
  );
  const currentPage =
    Number.isFinite(requestedPage) && requestedPage > 0 ? requestedPage : 1;
  const accountSummaryPromise = apiClient.GET("/accounts/summary");
  const fundSummaryPromise = apiClient.GET("/funds/summary");
  const openAccountingPeriodsPromise = apiClient.GET(
    "/accounting-periods/open",
  );
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: null,
        Limit: 1,
        Offset: 0,
      },
    },
  });
  const accountsPromise = apiClient.GET("/accounts", {
    params: {
      query: {
        Search: "",
        Sort: null,
        Limit: 1,
        Offset: 0,
      },
    },
  });
  const transactionsPromise = apiClient.GET("/transactions/unposted", {
    params: {
      query: {
        Search: "",
        Sort: null,
        Limit: 10,
        Offset: (currentPage - 1) * 10,
      },
    },
  });
  const fundsPromise = apiClient.GET("/funds", {
    params: {
      query: {
        Search: "",
        Sort: null,
        Limit: 1,
        Offset: 0,
      },
    },
  });

  const [
    { data: accountSummary },
    { data: fundSummary },
    { data: openAccountingPeriods },
    { data: accountingPeriods },
    { data: accounts },
    { data: funds },
    { data: transactions },
  ] = await Promise.all([
    accountSummaryPromise,
    fundSummaryPromise,
    openAccountingPeriodsPromise,
    accountingPeriodsPromise,
    accountsPromise,
    fundsPromise,
    transactionsPromise,
  ]);

  if (
    typeof accountSummary === "undefined" ||
    typeof fundSummary === "undefined" ||
    typeof openAccountingPeriods === "undefined" ||
    typeof accountingPeriods === "undefined" ||
    typeof accounts === "undefined" ||
    typeof funds === "undefined" ||
    typeof transactions === "undefined"
  ) {
    throw new Error("Failed to fetch overview data");
  }

  const unpostedTransactions = transactions.items;

  return {
    accountSummary,
    fundSummary,
    currentAccountingPeriod: openAccountingPeriods[0] ?? null,
    openAccountingPeriods,
    totalAccountingPeriods: accountingPeriods.totalCount,
    totalAccounts: accounts.totalCount,
    totalFunds: funds.totalCount,
    unpostedTransactions,
    unpostedTransactionTotalCount: transactions.totalCount,
  };
};

interface OverviewViewProps {
  readonly searchParams: Promise<{ page?: string | string[] }>;
}

/**
 * Component that displays the Overview view.
 */
const OverviewView = async function ({
  searchParams,
}: OverviewViewProps): Promise<JSX.Element> {
  const data = await getOverviewData(searchParams);

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Paper
        sx={{
          border: "1px solid",
          borderColor: "divider",
          p: { xs: 3, md: 4 },
          maxWidth: 1440,
        }}
      >
        <Stack spacing={1.5}>
          <Typography variant="overline" color="text.secondary">
            Financial Tracker
          </Typography>
          <Typography variant="h3">Overview</Typography>
        </Stack>
      </Paper>

      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: {
            xs: "1fr",
            lg: "repeat(1, minmax(0, 1fr))",
          },
        }}
      >
        <AccountingPeriodOverview />
        <Box
          sx={{
            display: "grid",
            gap: 3,
            gridTemplateColumns: { xs: "1fr", md: "repeat(2, minmax(0, 1fr))" },
          }}
        >
          <AccountOverview data={data} />
          <FundOverview data={data} />
        </Box>
        <GoalOverview />
      </Box>

      <TransactionOverview
        transactions={data.unpostedTransactions}
        totalCount={data.unpostedTransactionTotalCount}
      />
    </Stack>
  );
};

export default OverviewView;
