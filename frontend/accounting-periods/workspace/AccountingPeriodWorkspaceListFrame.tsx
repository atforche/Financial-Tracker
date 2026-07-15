"use client";

import {
  type AccountingPeriodWithBalance,
  AccountingPeriodWithBalanceSort,
  type AccountingPeriodWithBalanceSortValue,
} from "@/accounting-periods/types";
import { Button, Checkbox, Paper, Stack, Typography } from "@mui/material";
import { useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/framework/data/tryParseEnum";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the AccountingPeriodWorkspaceListFrame component.
 */
interface AccountingPeriodWorkspaceListFrameProps {
  readonly data: AccountingPeriodWithBalance[] | null;
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

  const sortParamName = "sort";
  const pageParamName = "page";
  const selectedAccountingPeriodIdParamName = "selectedAccountingPeriodId";
  const actionParamName = "action";
  const yearParamName = "years";
  const monthParamName = "months";

  const updateParams = useSearchParamUpdater([]);

  const setSort = function (
    sort: AccountingPeriodWithBalanceSortValue | null,
  ): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
      params.delete(pageParamName);
    });
  };

  const toggleSelection = function (accountId: string): void {
    updateParams((params) => {
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
    AccountingPeriodWithBalanceSort,
    searchParams.get(sortParamName) ?? "",
  );

  const getSortProps = createColumnSortProps(currentSort, setSort);

  const columns: ColumnDefinition<AccountingPeriodWithBalance>[] = [
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
      ...getSortProps(
        AccountingPeriodWithBalanceSort.Date,
        AccountingPeriodWithBalanceSort.DateDescending,
      ),
    },
    {
      name: "isOpen",
      headerContent: "Is Open",
      getBodyContent: (accountingPeriod) => (
        <Checkbox checked={accountingPeriod.isOpen} />
      ),
      ...getSortProps(
        AccountingPeriodWithBalanceSort.IsOpen,
        AccountingPeriodWithBalanceSort.IsOpenDescending,
      ),
      alignment: "center",
    },
    {
      name: "openingBalance",
      headerContent: "Opening Balance",
      getBodyContent: (accountingPeriod) =>
        formatCurrency(accountingPeriod.openingBalance),
      ...getSortProps(
        AccountingPeriodWithBalanceSort.OpeningBalance,
        AccountingPeriodWithBalanceSort.OpeningBalanceDescending,
      ),
      alignment: "right",
    },
    {
      name: "closingBalance",
      headerContent: "Closing Balance",
      getBodyContent: (accountingPeriod) =>
        formatCurrency(accountingPeriod.closingBalance),
      ...getSortProps(
        AccountingPeriodWithBalanceSort.ClosingBalance,
        AccountingPeriodWithBalanceSort.ClosingBalanceDescending,
      ),
      alignment: "right",
    },
  ];

  return (
    <ListFrame<AccountingPeriodWithBalance>
      title="Accounting Periods"
      columns={columns}
      getId={(accountingPeriod) => accountingPeriod.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
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
              updateParams((params) => {
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
  );
};

export default AccountingPeriodWorkspaceListFrame;
