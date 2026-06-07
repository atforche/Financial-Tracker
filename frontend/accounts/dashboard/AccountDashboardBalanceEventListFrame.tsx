"use client";

import {
  type AccountDashboardBalanceEvent,
  AccountDashboardBalanceEventSortOrder,
  AccountDashboardBalanceEventType,
  AccountDashboardMode,
} from "@/accounts/types";
import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/framework/data/tryParseEnum";

const dateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "numeric",
  year: "numeric",
});

const formatBalanceEventType = function (
  type: AccountDashboardBalanceEventType,
): string {
  return type === AccountDashboardBalanceEventType.Debit ? "Debit" : "Credit";
};

/**
 * Props for the AccountDashboardBalanceEventListFrame component.
 */
interface AccountDashboardBalanceEventListFrameProps {
  readonly data: AccountDashboardBalanceEvent[] | null;
  readonly totalCount: number | null;
  readonly mode: AccountDashboardMode;
}

/**
 * Presents the paged balance event table for the Accounts dashboard.
 */
const AccountDashboardBalanceEventListFrame = function ({
  data,
  mode,
  totalCount,
}: AccountDashboardBalanceEventListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "balanceEventSort";
  const pageParamName = "balanceEventPage";
  const accountTypeParamName = "accountType";
  const accountNameParamName = "accountName";
  const modeParamName = "mode";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";
  const startDateParamName = "startDate";
  const endDateParamName = "endDate";

  const setSort = function (
    sort: AccountDashboardBalanceEventSortOrder | null,
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

  const currentSort = tryParseEnum(
    AccountDashboardBalanceEventSortOrder,
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

  const columns: ColumnDefinition<AccountDashboardBalanceEvent>[] = [
    {
      name: "accountName",
      headerContent: "Account",
      getBodyContent: (balanceEvent) => balanceEvent.accountName,
      sortType:
        currentSort === AccountDashboardBalanceEventSortOrder.AccountName
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountDashboardBalanceEventSortOrder.AccountNameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardBalanceEventSortOrder.AccountName);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardBalanceEventSortOrder.AccountNameDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 140,
    },
    {
      name: "date",
      headerContent: "Event Date",
      getBodyContent: (balanceEvent) =>
        balanceEvent.isPosted
          ? dateFormatter.format(new Date(`${balanceEvent.date}T00:00:00`))
          : "Pending",
      sortType:
        currentSort === AccountDashboardBalanceEventSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort === AccountDashboardBalanceEventSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardBalanceEventSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardBalanceEventSortOrder.DateDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 130,
    },
    {
      name: "type",
      headerContent: "Type",
      getBodyContent: (balanceEvent): JSX.Element => (
        <Box
          component="span"
          sx={{
            color:
              balanceEvent.type === AccountDashboardBalanceEventType.Debit
                ? "warning.dark"
                : "info.dark",
            fontWeight: 600,
          }}
        >
          {formatBalanceEventType(balanceEvent.type)}
        </Box>
      ),
      sortType:
        currentSort === AccountDashboardBalanceEventSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === AccountDashboardBalanceEventSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardBalanceEventSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardBalanceEventSortOrder.TypeDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 90,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (balanceEvent) => formatCurrency(balanceEvent.amount),
      sortType:
        currentSort === AccountDashboardBalanceEventSortOrder.Amount
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountDashboardBalanceEventSortOrder.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardBalanceEventSortOrder.Amount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardBalanceEventSortOrder.AmountDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 120,
    },
  ];

  if (mode === AccountDashboardMode.AccountingPeriod) {
    columns.splice(1, 0, {
      name: "accountingPeriodName",
      headerContent: "Accounting Period",
      getBodyContent: (balanceEvent) => balanceEvent.accountingPeriodName,
      sortType:
        currentSort ===
        AccountDashboardBalanceEventSortOrder.AccountingPeriodName
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountDashboardBalanceEventSortOrder.AccountingPeriodNameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardBalanceEventSortOrder.AccountingPeriodName);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountDashboardBalanceEventSortOrder.AccountingPeriodNameDescending,
          );
        } else {
          setSort(null);
        }
      },
      minWidth: 160,
    });
  }

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2.5}>
        <Typography variant="h5">Balance Events</Typography>
        <ListFrame<AccountDashboardBalanceEvent>
          columns={columns}
          getId={(balanceEvent) =>
            `${balanceEvent.accountId}-${balanceEvent.accountingPeriodId}-${balanceEvent.date}-${balanceEvent.type}-${balanceEvent.amount}`
          }
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName="balanceEventSearch"
          pageParamName={pageParamName}
          hasActiveFilters={hasActiveFilters}
          initialEmptyState={{
            title: "No balance events found",
            description:
              "Try a different date range or accounting period to inspect account activity.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.replace(pathname);
                }}
              >
                Reset dashboard
              </Button>
            ),
          }}
          filteredEmptyState={{
            title: "No balance events match this dashboard filter",
            description:
              "Try a different account filter or range to widen the activity feed.",
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

export default AccountDashboardBalanceEventListFrame;
