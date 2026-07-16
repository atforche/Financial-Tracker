"use client";

import {
  type AccountingPeriodWithBalance,
  AccountingPeriodWithBalanceSort,
} from "@/accounting-periods/types";
import { Button, Checkbox, IconButton, Stack } from "@mui/material";
import { useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriodTrendsSearchParams } from "@/accounting-periods/trends/AccountingPeriodTrends";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import FilterListOutlined from "@mui/icons-material/FilterListOutlined";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatCurrency } from "@/framework/currencyHelpers";
import nameof from "@/framework/data/nameof";
import routes from "@/accounting-periods/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the AccountingPeriodTrendsListFrame component.
 */
interface AccountingPeriodTrendsListFrameProps {
  readonly data: AccountingPeriodWithBalance[] | null;
  readonly totalCount: number | null;
}

/**
 * Presents the paged accounting period table for the Accounting Periods trends.
 */
const AccountingPeriodTrendsListFrame = function ({
  data,
  totalCount,
}: AccountingPeriodTrendsListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const router = useRouter();

  const startAccountingPeriodParamName =
    nameof<AccountingPeriodTrendsSearchParams>("startAccountingPeriodId");
  const endAccountingPeriodParamName =
    nameof<AccountingPeriodTrendsSearchParams>("endAccountingPeriodId");
  const sortParamName = nameof<AccountingPeriodTrendsSearchParams>("sort");
  const pageParamName = nameof<AccountingPeriodTrendsSearchParams>("page");
  const updateParams = useSearchParamUpdater([pageParamName]);

  const setSort = function (
    sort: AccountingPeriodWithBalanceSort | null,
  ): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const setAccountingPeriodFilter = function (
    accountingPeriod: AccountingPeriodWithBalance,
  ): void {
    updateParams((params) => {
      params.delete(startAccountingPeriodParamName);
      params.append(startAccountingPeriodParamName, accountingPeriod.id);
      params.delete(endAccountingPeriodParamName);
      params.append(endAccountingPeriodParamName, accountingPeriod.id);
    });
  };

  const currentSort = tryParseEnum(
    AccountingPeriodWithBalanceSort,
    searchParams.get(sortParamName) ?? "",
  );

  const openAccountingPeriodWorkspace = function (
    accountingPeriod: AccountingPeriodWithBalance,
  ): void {
    router.push(
      routes.workspace({
        years: [accountingPeriod.year],
        months: [accountingPeriod.month],
        selectedAccountingPeriodId: accountingPeriod.id,
      }),
    );
  };

  const getSortProps = createColumnSortProps(currentSort, setSort);

  const columns: ColumnDefinition<AccountingPeriodWithBalance>[] = [
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
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (accountingPeriod) => (
        <Stack direction="row" spacing={0.5} justifyContent="flex-end">
          <IconButton
            size="small"
            color="inherit"
            onClick={(event) => {
              event.stopPropagation();
              setAccountingPeriodFilter(accountingPeriod);
            }}
            aria-label={`Filter ${accountingPeriod.name}`}
          >
            <FilterListOutlined fontSize="small" color="action" />
          </IconButton>
          <IconButton
            size="small"
            color="primary"
            onClick={(event) => {
              event.stopPropagation();
              openAccountingPeriodWorkspace(accountingPeriod);
            }}
            aria-label={`Open ${accountingPeriod.name}`}
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
    <ListFrame<AccountingPeriodWithBalance>
      title="Accounting Periods"
      columns={columns}
      getId={(accountingPeriod) => accountingPeriod.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
      pageParamName={pageParamName}
      onRowClick={(accountingPeriod: AccountingPeriodWithBalance): void => {
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
              updateParams((params) => {
                [...params.keys()].forEach((key) => {
                  params.delete(key);
                });
              });
            }}
          >
            Clear search
          </Button>
        ),
      }}
    />
  );
};

export default AccountingPeriodTrendsListFrame;
