import {
  type AccountSortOrder,
  type AccountTypeBalance,
  formatAccountType,
} from "@/accounts/types";
import { Box, Divider, Stack, Typography } from "@mui/material";
import AccountListFrame from "@/accounts/AccountListFrame";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import SummaryCard from "@/framework/view/SummaryCard";
import breadcrumbs from "@/accounts/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the AccountsView component.
 */
interface AccountsViewSearchParams {
  search?: string;
  sort?: AccountSortOrder;
  page?: number;
}

/**
 * Props for the AccountsView component.
 */
interface AccountsViewProps {
  readonly searchParams: Promise<AccountsViewSearchParams>;
}

/**
 * Component that displays the Accounts view.
 */
const AccountsView = async function ({
  searchParams,
}: AccountsViewProps): Promise<JSX.Element> {
  const { search, sort, page } = await searchParams;

  const apiClient = getApiClient();
  const accountsPromise = apiClient.GET("/accounts", {
    params: {
      query: {
        Search: search ?? "",
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: ((page ?? 1) - 1) * rowsPerPage,
      },
    },
  });
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
  const summaryPromise = apiClient.GET("/accounts/summary");

  const [{ data: accounts }, { data: accountingPeriods }, { data: summary }] =
    await Promise.all([
      accountsPromise,
      accountingPeriodsPromise,
      summaryPromise,
    ]);
  if (
    typeof accounts === "undefined" ||
    typeof accountingPeriods === "undefined" ||
    typeof summary === "undefined"
  ) {
    throw new Error(`Failed to fetch accounts`);
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns: {
            xs: "1fr",
            md: "repeat(4, minmax(0, 1fr))",
          },
        }}
      >
        <SummaryCard
          title="Total Balance"
          value={formatCurrency(summary.totalBalance)}
          description="Sum of all account balances"
        />
        <SummaryCard
          title="Total Tracked Balance"
          value={formatCurrency(summary.totalTrackedBalance)}
          description="Sum of tracked account balances"
        />
        <SummaryCard
          title="Total Untracked Balance"
          value={formatCurrency(summary.totalUntrackedBalance)}
          description="Sum of untracked account balances"
        />
        <SummaryCard
          title="Balance by Account Type"
          description="Posted balance totals grouped by account type"
        >
          <Stack divider={<Divider flexItem />}>
            {summary.balanceByAccountType.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No account balances available.
              </Typography>
            ) : (
              summary.balanceByAccountType.map(
                (balanceByAccountType: AccountTypeBalance) => (
                  <Box
                    key={balanceByAccountType.accountType}
                    sx={{
                      display: "flex",
                      justifyContent: "space-between",
                      gap: 2,
                      py: 1,
                    }}
                  >
                    <Typography variant="body1">
                      {formatAccountType(balanceByAccountType.accountType)}
                    </Typography>
                    <Typography variant="body1" fontWeight={600}>
                      {formatCurrency(balanceByAccountType.totalBalance)}
                    </Typography>
                  </Box>
                ),
              )
            )}
          </Stack>
        </SummaryCard>
      </Box>
      <SearchBar
        searchParamName={nameof<AccountsViewSearchParams>("search")}
        pageParamName={nameof<AccountsViewSearchParams>("page")}
      />
      <AccountListFrame
        data={accounts.items}
        isInOnboardingMode={accountingPeriods.totalCount === 0}
        totalCount={accounts.totalCount}
      />
    </Stack>
  );
};

export type { AccountsViewSearchParams };
export default AccountsView;
