"use client";

import {
  type AccountWithBalanceRange,
  AccountWithBalanceRangeSort,
  type AccountWithBalanceRangeSortValue,
  formatAccountType,
  isPositiveChangeInBalance,
} from "@/accounts/types";
import {
  Box,
  Button,
  IconButton,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import FilterListOutlined from "@mui/icons-material/FilterListOutlined";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/accounts/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the AccountTrendsListFrame component.
 */
interface AccountTrendsListFrameProps {
  readonly data: AccountWithBalanceRange[] | null;
  readonly totalCount: number | null;
  readonly isInOnboardingMode: boolean;
}

/**
 * Presents the paged account table for the Accounts trends.
 */
const AccountTrendsListFrame = function ({
  data,
  totalCount,
  isInOnboardingMode,
}: AccountTrendsListFrameProps): JSX.Element {
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

  const setSort = function (
    sort: AccountWithBalanceRangeSortValue | null,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortParamName);
    } else {
      params.set(sortParamName, sort);
    }
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  };

  const setAccountNameFilter = function (accountName: string): void {
    const params = new URLSearchParams(searchParams.toString());
    params.delete(accountNameParamName);
    params.append(accountNameParamName, accountName);
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  };

  const openAccountWorkspace = function (
    account: AccountWithBalanceRange,
  ): void {
    router.push(routes.workspaceDetail(account.id, {}));
  };

  const currentSort = tryParseEnum(
    AccountWithBalanceRangeSort,
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

  const columns: ColumnDefinition<AccountWithBalanceRange>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (account) => account.name,
      sortType:
        currentSort === AccountWithBalanceRangeSort.Name
          ? ColumnSortType.Ascending
          : currentSort === AccountWithBalanceRangeSort.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountWithBalanceRangeSort.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountWithBalanceRangeSort.NameDescending);
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
        currentSort === AccountWithBalanceRangeSort.Type
          ? ColumnSortType.Ascending
          : currentSort === AccountWithBalanceRangeSort.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountWithBalanceRangeSort.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountWithBalanceRangeSort.TypeDescending);
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
        currentSort === AccountWithBalanceRangeSort.StartingBalance
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountWithBalanceRangeSort.StartingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountWithBalanceRangeSort.StartingBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountWithBalanceRangeSort.StartingBalanceDescending);
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
        currentSort === AccountWithBalanceRangeSort.EndingBalance
          ? ColumnSortType.Ascending
          : currentSort === AccountWithBalanceRangeSort.EndingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountWithBalanceRangeSort.EndingBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountWithBalanceRangeSort.EndingBalanceDescending);
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
        currentSort === AccountWithBalanceRangeSort.NetChange
          ? ColumnSortType.Ascending
          : currentSort === AccountWithBalanceRangeSort.NetChangeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountWithBalanceRangeSort.NetChange);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountWithBalanceRangeSort.NetChangeDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 160,
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (account) => (
        <Stack direction="row" spacing={0.5} justifyContent="flex-end">
          <IconButton
            size="small"
            color="inherit"
            onClick={(event) => {
              event.stopPropagation();
              setAccountNameFilter(account.name);
            }}
            aria-label={`Filter ${account.name}`}
          >
            <FilterListOutlined fontSize="small" color="action" />
          </IconButton>
          <IconButton
            size="small"
            color="primary"
            onClick={(event) => {
              event.stopPropagation();
              openAccountWorkspace(account);
            }}
            aria-label={`Open ${account.name}`}
          >
            <ArrowForwardOutlined fontSize="small" color="action" />
          </IconButton>
        </Stack>
      ),
      alignment: "right",
      minWidth: 84,
      maxWidth: 84,
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
        <ListFrame<AccountWithBalanceRange>
          columns={columns}
          getId={(account) => account.id}
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName="search"
          pageParamName={pageParamName}
          onRowClick={(account: AccountWithBalanceRange): void => {
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
            title: "No accounts match this trends filter",
            description:
              "Try a different account type, account name, or date range to widen the trends scope.",
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

export default AccountTrendsListFrame;
