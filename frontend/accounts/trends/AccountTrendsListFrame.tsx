"use client";

import {
  type AccountWithBalanceRange,
  AccountWithBalanceRangeSort,
} from "@/accounts/types";
import { Box, Button, Stack } from "@mui/material";
import {
  accountTrendsParamNames,
  clearAccountTrendsFilters,
  hasActiveAccountTrendsFilters,
} from "@/accounts/trends/helpers";
import {
  formatAccountType,
  isPositiveChangeInBalance,
} from "@/accounts/helpers";
import { useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import FilterListOutlined from "@mui/icons-material/FilterListOutlined";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatCurrency } from "@/framework/currencyHelpers";
import parseEnumValue from "@/framework/data/parseEnumValue";
import routes from "@/accounts/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the AccountTrendsListFrame component.
 */
interface AccountTrendsListFrameProps {
  readonly data: readonly AccountWithBalanceRange[];
  readonly totalCount: number;
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
  const router = useRouter();

  const { sort: sortParamName, page: pageParamName } = accountTrendsParamNames;
  const accountNameParamName = accountTrendsParamNames.accountName;
  const updateParams = useSearchParamUpdater([pageParamName]);
  const updateFilters = useSearchParamUpdater([
    pageParamName,
    accountTrendsParamNames.balanceEventPage,
  ]);

  const setSort = function (sort: AccountWithBalanceRangeSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const setAccountNameFilter = function (accountName: string): void {
    updateFilters((params) => {
      params.delete(accountNameParamName);
      params.append(accountNameParamName, accountName);
    });
  };

  const openAccountWorkspace = function (
    account: AccountWithBalanceRange,
  ): void {
    router.push(routes.workspaceDetail(account.id, {}));
  };

  const currentSort = parseEnumValue(
    AccountWithBalanceRangeSort,
    searchParams.get(sortParamName) ?? "",
  );
  const hasActiveFilters = hasActiveAccountTrendsFilters(searchParams);

  const getSortProps = createColumnSortProps(currentSort, setSort);

  const columns: ColumnDefinition<AccountWithBalanceRange>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (account) => account.name,
      ...getSortProps(
        AccountWithBalanceRangeSort.Name,
        AccountWithBalanceRangeSort.NameDescending,
      ),
    },
    {
      name: "type",
      headerContent: "Type",
      getBodyContent: (account) => formatAccountType(account.type),
      ...getSortProps(
        AccountWithBalanceRangeSort.Type,
        AccountWithBalanceRangeSort.TypeDescending,
      ),
    },
    {
      name: "startingBalance",
      headerContent: "Starting Balance",
      getBodyContent: (account) => formatCurrency(account.startingBalance),
      ...getSortProps(
        AccountWithBalanceRangeSort.StartingBalance,
        AccountWithBalanceRangeSort.StartingBalanceDescending,
      ),
      alignment: "right",
      minWidth: 140,
    },
    {
      name: "endingBalance",
      headerContent: "Ending Balance",
      getBodyContent: (account) => formatCurrency(account.endingBalance),
      ...getSortProps(
        AccountWithBalanceRangeSort.EndingBalance,
        AccountWithBalanceRangeSort.EndingBalanceDescending,
      ),
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
      ...getSortProps(
        AccountWithBalanceRangeSort.NetChange,
        AccountWithBalanceRangeSort.NetChangeDescending,
      ),
      alignment: "right",
      minWidth: 160,
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (account) => (
        <Stack direction="row" spacing={0.5} justifyContent="flex-end">
          <ListFrameActionButton
            size="small"
            color="inherit"
            onClick={(event) => {
              event.stopPropagation();
              setAccountNameFilter(account.name);
            }}
            ariaLabel={`Filter ${account.name}`}
          >
            <FilterListOutlined fontSize="small" color="action" />
          </ListFrameActionButton>
          <ListFrameActionButton
            size="small"
            color="primary"
            onClick={(event) => {
              event.stopPropagation();
              openAccountWorkspace(account);
            }}
            ariaLabel={`Open ${account.name}`}
          >
            <ArrowForwardOutlined fontSize="small" color="action" />
          </ListFrameActionButton>
        </Stack>
      ),
      alignment: "right",
      minWidth: 84,
      maxWidth: 84,
    },
  ];

  return (
    <ListFrame<AccountWithBalanceRange>
      title="Accounts"
      columns={columns}
      getId={(account) => account.id}
      data={data}
      totalCount={totalCount}
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
              updateParams((params) => {
                clearAccountTrendsFilters(params);
              });
            }}
          >
            Reset filters
          </Button>
        ),
      }}
    />
  );
};

export default AccountTrendsListFrame;
