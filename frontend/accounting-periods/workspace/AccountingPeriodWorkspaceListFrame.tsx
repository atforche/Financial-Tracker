"use client";

import type {
  AccountingPeriod,
  AccountingPeriodSortOrder,
} from "@/accounting-periods/types";
import { Button, Checkbox, Paper, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Props for the AccountingPeriodWorkspaceListFrame component.
 */
interface AccountingPeriodWorkspaceListFrameProps {
  readonly data: AccountingPeriod[] | null;
  readonly totalCount: number | null;
  readonly selectedAccountingPeriodId: string | null;
}

const parsePageNumber = function (page: string | null): number {
  const pageNumber = Number.parseInt(page ?? "1", 10);
  return Number.isFinite(pageNumber) && pageNumber > 0 ? pageNumber : 1;
};

/**
 * Displays the accounting periods that fall within the current workspace range.
 */
const AccountingPeriodWorkspaceListFrame = function ({
  accountingPeriods,
  defaultAccountingPeriodId,
}: AccountingPeriodWorkspaceListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "sort";
  const pageParamName = "page";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";

  const replaceSearchParams = function (
    update: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    update(params);
    router.replace(`${pathname}?${params.toString()}`);
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

  const currentStartAccountingPeriodId =
    searchParams.get(startAccountingPeriodIdParamName) ??
    defaultAccountingPeriodId ??
    "";
  const currentEndAccountingPeriodId =
    searchParams.get(endAccountingPeriodIdParamName) ??
    defaultAccountingPeriodId ??
    "";

  const currentPage = parsePageNumber(searchParams.get(pageParamName));
  const accountingPeriodIndexes = new Map(
    accountingPeriods.map((accountingPeriod, index) => [
      accountingPeriod.id,
      index,
    ]),
  );

  const currentStartIndex =
    accountingPeriodIndexes.get(currentStartAccountingPeriodId) ?? 0;
  const currentEndIndex =
    accountingPeriodIndexes.get(currentEndAccountingPeriodId) ??
    currentStartIndex;
  const lowerBound = Math.min(currentStartIndex, currentEndIndex);
  const upperBound = Math.max(currentStartIndex, currentEndIndex);
  const filteredAccountingPeriods = accountingPeriods.filter(
    (_, index) => index >= lowerBound && index <= upperBound,
  );
  const pagedAccountingPeriods = filteredAccountingPeriods.slice(
    (currentPage - 1) * rowsPerPage,
    currentPage * rowsPerPage,
  );

  const hasActiveFilters =
    currentStartAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentEndAccountingPeriodId !== (defaultAccountingPeriodId ?? "");

  const columns: ColumnDefinition<AccountingPeriod>[] = [
    {
      name: "selected",
      headerContent: "",
      getBodyContent: (account) => (
        <Checkbox
          checked={selectedAccountId === account.id}
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
    },
    {
      name: "isOpen",
      headerContent: "Is Open",
      getBodyContent: (accountingPeriod) => (
        <Checkbox checked={accountingPeriod.isOpen} />
      ),
      alignment: "center",
    },
    {
      name: "openingBalance",
      headerContent: "Opening Balance",
      getBodyContent: (accountingPeriod) =>
        formatCurrency(accountingPeriod.openingBalance),
      alignment: "right",
    },
    {
      name: "closingBalance",
      headerContent: "Closing Balance",
      getBodyContent: (accountingPeriod) =>
        formatCurrency(accountingPeriod.closingBalance),
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
      <Stack spacing={2}>
        <Typography variant="h5">Accounting periods</Typography>
        <ListFrame<AccountingPeriod>
          columns={columns}
          getId={(accountingPeriod) => accountingPeriod.id}
          data={pagedAccountingPeriods}
          totalCount={filteredAccountingPeriods.length}
          searchParamName="search"
          pageParamName={pageParamName}
          hasActiveFilters={hasActiveFilters}
          initialEmptyState={{
            title: "No accounting periods yet",
            description:
              "Use the create action to add the first accounting period.",
            action: null,
          }}
          filteredEmptyState={{
            title: "No accounting periods fall within this range",
            description:
              "Try a wider accounting period range to include more periods.",
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

export default AccountingPeriodWorkspaceListFrame;
