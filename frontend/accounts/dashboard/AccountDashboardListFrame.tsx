"use client";

import {
  type AccountDashboardAccount,
  AccountDashboardSortOrder,
  formatAccountType,
  isPositiveChangeInBalance,
} from "@/accounts/types";
import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/accounts/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the AccountDashboardListFrame component.
 */
interface AccountDashboardListFrameProps {
  readonly data: AccountDashboardAccount[] | null;
  readonly totalCount: number | null;
  readonly isInOnboardingMode: boolean;
}

/**
 * Presents the paged account table for the Accounts dashboard.
 */
const AccountDashboardListFrame = function ({
  data,
  totalCount,
  isInOnboardingMode,
}: AccountDashboardListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "sort";
  const pageParamName = "page";
  const accountTypeParamName = "accountType";
  const accountNameParamName = "accountName";
  const modeParamName = "mode";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";
  const startDateParamName = "startDate";
  const endDateParamName = "endDate";

  const setSort = function (sort: AccountDashboardSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortParamName);
    } else {
      params.set(sortParamName, sort);
    }
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`);
  };

  const setAccountNameFilter = function (accountName: string): void {
    const params = new URLSearchParams(searchParams.toString());
    params.delete(accountNameParamName);
    params.append(accountNameParamName, accountName);
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountDashboardSortOrder,
    searchParams.get(sortParamName) ?? "",
  );
  const hasActiveFilters =
    searchParams.getAll(accountTypeParamName).length > 0 ||
    searchParams.getAll(accountNameParamName).length > 0 ||
    searchParams.get(modeParamName) === "date" ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName) ||
    searchParams.has(startDateParamName) ||
    searchParams.has(endDateParamName);

  const columns: ColumnDefinition<AccountDashboardAccount>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (account) => account.name,
      sortType:
        currentSort === AccountDashboardSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === AccountDashboardSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardSortOrder.NameDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "type",
      headerContent: "Type",
      getBodyContent: (account) => formatAccountType(account.type),
      sortType:
        currentSort === AccountDashboardSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === AccountDashboardSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardSortOrder.TypeDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "startingBalance",
      headerContent: "Starting Balance",
      getBodyContent: (account) => formatCurrency(account.startingBalance),
      sortType:
        currentSort === AccountDashboardSortOrder.OpeningBalance
          ? ColumnSortType.Ascending
          : currentSort === AccountDashboardSortOrder.OpeningBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardSortOrder.OpeningBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardSortOrder.OpeningBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 140,
    },
    {
      name: "endingBalance",
      headerContent: "Ending Balance",
      getBodyContent: (account) => formatCurrency(account.endingBalance),
      sortType:
        currentSort === AccountDashboardSortOrder.ClosingBalance
          ? ColumnSortType.Ascending
          : currentSort === AccountDashboardSortOrder.ClosingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardSortOrder.ClosingBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardSortOrder.ClosingBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 140,
    },
    {
      name: "change",
      headerContent: "Net Change",
      getBodyContent: (account): JSX.Element => {
        const changeInBalance = account.endingBalance - account.startingBalance;
        const isPositive = isPositiveChangeInBalance(
          account.type,
          changeInBalance,
        );
        return (
          <Box
            component="span"
            sx={{
              color: isPositive ? "success.main" : "error.main",
              display: "inline",
            }}
          >
            {formatCurrency(changeInBalance)}
          </Box>
        );
      },
      sortType:
        currentSort === AccountDashboardSortOrder.NetChange
          ? ColumnSortType.Ascending
          : currentSort === AccountDashboardSortOrder.NetChangeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardSortOrder.NetChange);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardSortOrder.NetChangeDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 160,
    },
  ];

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2.5}>
        <Typography variant="h5">Accounts</Typography>
        <ListFrame<AccountDashboardAccount>
          columns={columns}
          getId={(account) => account.id}
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName="search"
          pageParamName={pageParamName}
          onRowClick={(account: AccountDashboardAccount): void => {
            setAccountNameFilter(account.name);
          }}
          hasActiveFilters={hasActiveFilters}
          initialEmptyState={{
            title: "No accounts have been added",
            description: isInOnboardingMode
              ? "Onboard a new account to start tracking balances."
              : "Create a new account to start tracking balances.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.push(
                    isInOnboardingMode
                      ? routes.workspace({ action: "onboard" })
                      : routes.workspace({ action: "create" }),
                  );
                }}
              >
                {isInOnboardingMode ? "Onboard account" : "Create account"}
              </Button>
            ),
          }}
          filteredEmptyState={{
            title: "No accounts match this dashboard filter",
            description:
              "Try a different account type, account name, or date range to widen the dashboard scope.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.replace(pathname);
                }}
              >
                Reset filters
              </Button>
            ),
          }}
        />
      </Stack>
    </Paper>
  );
};

export default AccountDashboardListFrame;
