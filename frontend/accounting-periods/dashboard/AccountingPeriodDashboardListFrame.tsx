"use client";

import {
  type AccountingPeriod,
  AccountingPeriodSortOrder,
} from "@/accounting-periods/types";
import { Button, Checkbox, Paper, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriodDashboardSearchParams } from "@/accounting-periods/dashboard/AccountingPeriodDashboard";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import nameof from "@/framework/data/nameof";
import routes from "@/accounting-periods/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the AccountingPeriodDashboardListFrame component.
 */
interface AccountingPeriodDashboardListFrameProps {
  readonly data: AccountingPeriod[] | null;
  readonly totalCount: number | null;
}

/**
 * Presents the paged accounting period table for the Accounting Periods dashboard.
 */
const AccountingPeriodDashboardListFrame = function ({
  data,
  totalCount,
}: AccountingPeriodDashboardListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const startAccountingPeriodParamName =
    nameof<AccountingPeriodDashboardSearchParams>("startAccountingPeriodId");
  const endAccountingPeriodParamName =
    nameof<AccountingPeriodDashboardSearchParams>("endAccountingPeriodId");
  const sortParamName = nameof<AccountingPeriodDashboardSearchParams>("sort");
  const pageParamName = nameof<AccountingPeriodDashboardSearchParams>("page");

  const setSort = function (sort: AccountingPeriodSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortParamName);
    } else {
      params.set(sortParamName, sort);
    }
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`);
  };

  const setAccountingPeriodFilter = function (
    accountingPeriod: AccountingPeriod,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    params.delete(startAccountingPeriodParamName);
    params.append(startAccountingPeriodParamName, accountingPeriod.id);
    params.delete(endAccountingPeriodParamName);
    params.append(endAccountingPeriodParamName, accountingPeriod.id);
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountingPeriodSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const columns: ColumnDefinition<AccountingPeriod>[] = [
    {
      name: "period",
      headerContent: "Period",
      getBodyContent: (accountingPeriod) => accountingPeriod.name,
      sortType:
        currentSort === AccountingPeriodSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodSortOrder.DateDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "isOpen",
      headerContent: "Is Open",
      getBodyContent: (accountingPeriod) => (
        <Checkbox checked={accountingPeriod.isOpen} />
      ),
      sortType:
        currentSort === AccountingPeriodSortOrder.IsOpen
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodSortOrder.IsOpenDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodSortOrder.IsOpen);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodSortOrder.IsOpenDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "openingBalance",
      headerContent: "Opening Balance",
      getBodyContent: (accountingPeriod) =>
        formatCurrency(accountingPeriod.openingBalance),
      sortType:
        currentSort === AccountingPeriodSortOrder.OpeningBalance
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodSortOrder.OpeningBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodSortOrder.OpeningBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodSortOrder.OpeningBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
    },
    {
      name: "closingBalance",
      headerContent: "Closing Balance",
      getBodyContent: (accountingPeriod) =>
        formatCurrency(accountingPeriod.closingBalance),
      sortType:
        currentSort === AccountingPeriodSortOrder.ClosingBalance
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodSortOrder.ClosingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodSortOrder.ClosingBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodSortOrder.ClosingBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
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
        <Typography variant="h5">Accounting periods</Typography>
        <ListFrame<AccountingPeriod>
          columns={columns}
          getId={(accountingPeriod) => accountingPeriod.id}
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName="search"
          pageParamName={pageParamName}
          onRowClick={(accountingPeriod: AccountingPeriod): void => {
            setAccountingPeriodFilter(accountingPeriod);
          }}
          initialEmptyState={{
            title: "No accounting periods have been added",
            description:
              "Create an accounting period to start organizing balances by month.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.push(routes.workspace({ action: "create" }));
                }}
              >
                Create accounting period
              </Button>
            ),
          }}
          filteredEmptyState={{
            title: "No accounting periods match this search",
            description:
              "Try a different month or year, or clear the current search to see all accounting periods.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.replace(pathname);
                }}
              >
                Clear search
              </Button>
            ),
          }}
        />
      </Stack>
    </Paper>
  );
};

export default AccountingPeriodDashboardListFrame;
