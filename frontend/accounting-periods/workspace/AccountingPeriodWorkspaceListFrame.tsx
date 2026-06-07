"use client";

import {
  type AccountingPeriod,
  AccountingPeriodSortOrder,
} from "@/accounting-periods/types";
import { Button, Checkbox, Paper, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the AccountingPeriodWorkspaceListFrame component.
 */
interface AccountingPeriodWorkspaceListFrameProps {
  readonly data: AccountingPeriod[] | null;
  readonly totalCount: number | null;
  readonly selectedAccountingPeriodId: string | null;
}

/**
 * Displays the accounting periods that fall within the current workspace range.
 */
const AccountingPeriodWorkspaceListFrame = function ({
  data,
  totalCount,
  selectedAccountingPeriodId,
}: AccountingPeriodWorkspaceListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "sort";
  const pageParamName = "page";
  const selectedAccountingPeriodIdParamName = "selectedAccountingPeriodId";
  const actionParamName = "action";
  const yearParamName = "years";
  const monthParamName = "months";

  const replaceSearchParams = function (
    update: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    update(params);
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  };

  const setSort = function (sort: AccountingPeriodSortOrder | null): void {
    replaceSearchParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
      params.delete(pageParamName);
    });
  };

  const toggleSelection = function (accountId: string): void {
    replaceSearchParams((params) => {
      const currentlySelectedAccountId = params.get(
        selectedAccountingPeriodIdParamName,
      );
      if (currentlySelectedAccountId === accountId) {
        params.delete(selectedAccountingPeriodIdParamName);
        return;
      }
      params.set(selectedAccountingPeriodIdParamName, accountId);
    });
  };

  const currentSort = tryParseEnum(
    AccountingPeriodSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const columns: ColumnDefinition<AccountingPeriod>[] = [
    {
      name: "selected",
      headerContent: "",
      getBodyContent: (account) => (
        <Checkbox
          checked={selectedAccountingPeriodId === account.id}
          onClick={(event) => {
            event.stopPropagation();
            toggleSelection(account.id);
          }}
          slotProps={{
            input: {
              "aria-label": `Select ${account.name}`,
            },
          }}
        />
      ),
      alignment: "center",
      minWidth: 0,
      maxWidth: 0,
    },
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
      alignment: "center",
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
        borderRadius: 3,
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2.5}>
        <Typography variant="h6" color="text.secondary">
          Accounting Periods
        </Typography>
        <ListFrame<AccountingPeriod>
          columns={columns}
          getId={(accountingPeriod) => accountingPeriod.id}
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName=""
          pageParamName={pageParamName}
          onRowClick={(accountingPeriod) => {
            toggleSelection(accountingPeriod.id);
          }}
          isRowSelected={(accountingPeriod) =>
            accountingPeriod.id === selectedAccountingPeriodId
          }
          initialEmptyState={{
            title: "No accounting periods yet",
            description:
              "Use the create action to add the first accounting period.",
            action: null,
          }}
          filteredEmptyState={{
            title: "No accounting periods match the current filters",
            description:
              "Try a wider accounting period range to include more periods.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  replaceSearchParams((params) => {
                    params.delete(yearParamName);
                    params.delete(monthParamName);
                    params.delete(pageParamName);
                    params.delete(selectedAccountingPeriodIdParamName);
                    if (
                      params.get(actionParamName) === "update" ||
                      params.get(actionParamName) === "delete"
                    ) {
                      params.delete(actionParamName);
                    }
                  });
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

export default AccountingPeriodWorkspaceListFrame;
