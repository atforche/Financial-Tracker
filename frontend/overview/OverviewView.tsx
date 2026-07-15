import { Box, Paper, Stack, Typography } from "@mui/material";
import AccountOverview from "@/overview/AccountOverview";
import AccountingPeriodOverview from "@/overview/AccountingPeriodOverview";
import FundOverview from "@/overview/FundOverview";
import GoalOverview from "@/overview/GoalOverview";
import type { JSX } from "react";
import type { OverviewData } from "@/overview/types";
import TransactionOverview from "@/overview/TransactionOverview";
import getApiClient from "@/framework/data/getApiClient";
import { AccountTypeModel } from "@/framework/data/api";

/**
 * Loads all data required by the overview page.
 */
const getOverviewData = async function (): Promise<OverviewData> {
  const apiClient = getApiClient();
  const accountSummaryPromise = apiClient.GET("/accounts/with-balances");
  const fundSummaryPromise = apiClient.GET("/funds/with-balances");
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Sort: null,
        Limit: 500,
        Offset: 0,
      },
    },
  });
  const accountsPromise = apiClient.GET("/accounts", {
    params: {
      query: {
        Sort: null,
        Limit: 1,
        Offset: 0,
      },
    },
  });
  const fundsPromise = apiClient.GET("/funds", {
    params: {
      query: {
        Sort: null,
        Limit: 1,
        Offset: 0,
      },
    },
  });

  const [
    { data: accountSummary },
    { data: fundSummary },
    { data: accountingPeriods },
    { data: accounts },
    { data: funds },
  ] = await Promise.all([
    accountSummaryPromise,
    fundSummaryPromise,
    accountingPeriodsPromise,
    accountsPromise,
    fundsPromise,
  ]);

  if (
    typeof accountSummary === "undefined" ||
    typeof fundSummary === "undefined" ||
    typeof accountingPeriods === "undefined" ||
    typeof accounts === "undefined" ||
    typeof funds === "undefined"
  ) {
    throw new Error("Failed to fetch overview data");
  }

  const accountBalances = accountSummary.items;
  const trackedAccounts = accountBalances.filter(
    (account) =>
      account.type === AccountTypeModel.Standard ||
      account.type === AccountTypeModel.CreditCard,
  );
  const balanceByAccountType = Array.from(
    Map.groupBy(accountBalances, (account) => account.type),
    ([accountType, groupedAccounts]) => ({
      accountType,
      totalBalance: groupedAccounts.reduce(
        (total, account) => total + account.currentBalance.postedBalance,
        0,
      ),
    }),
  );
  const openAccountingPeriods = accountingPeriods.items.filter(
    (period) => period.isOpen,
  );
  const assignedFunds = fundSummary.items.filter(
    (fund) => fund.name !== "Unassigned",
  );
  return {
    accountSummary: {
      totalBalance: accountBalances.reduce(
        (total, account) => total + account.currentBalance.postedBalance,
        0,
      ),
      totalTrackedBalance: trackedAccounts.reduce(
        (total, account) => total + account.currentBalance.postedBalance,
        0,
      ),
      totalUntrackedBalance: accountBalances
        .filter((account) => !trackedAccounts.includes(account))
        .reduce(
          (total, account) => total + account.currentBalance.postedBalance,
          0,
        ),
      balanceByAccountType,
    },
    fundSummary: {
      totalBalance: fundSummary.items.reduce(
        (total, fund) => total + fund.currentBalance.postedBalance,
        0,
      ),
      totalAssignedBalance: assignedFunds.reduce(
        (total, fund) => total + fund.currentBalance.postedBalance,
        0,
      ),
      totalUnassignedBalance:
        fundSummary.items.find((fund) => fund.name === "Unassigned")
          ?.currentBalance.postedBalance ?? 0,
    },
    currentAccountingPeriod: openAccountingPeriods[0] ?? null,
    openAccountingPeriods,
    totalAccountingPeriods: accountingPeriods.totalCount,
    totalAccounts: accounts.totalCount,
    totalFunds: funds.totalCount,
  };
};

/**
 * Component that displays the Overview view.
 */
const OverviewView = async function (): Promise<JSX.Element> {
  const data = await getOverviewData();

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
        currentAccountingPeriod={data.currentAccountingPeriod}
      />
    </Stack>
  );
};

export default OverviewView;
