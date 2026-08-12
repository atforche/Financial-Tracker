"use client";

import {
  type AccountingPeriodWithBalance,
  AccountingPeriodWithBalanceSort,
} from "@/accounting-periods/types";
import { Button, Checkbox } from "@mui/material";
import { useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatCurrency } from "@/framework/currencyHelpers";
import parseEnumValue from "@/framework/data/parseEnumValue";
import propertyName from "@/framework/data/propertyName";
import routes from "@/accounting-periods/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the AccountingPeriodWorkspaceListFrame component.
 */
interface AccountingPeriodWorkspaceListFrameProps {
  readonly data: AccountingPeriodWithBalance[] | null;
  readonly totalCount: number | null;
}

/**
 * Displays the accounting periods that fall within the current workspace range.
 */
const AccountingPeriodWorkspaceListFrame = function ({
  data,
  totalCount,
}: AccountingPeriodWorkspaceListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const router = useRouter();

  const sortParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("sort");
  const pageParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("page");
  const actionParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("action");
  const yearParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("years");
  const monthParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("months");
  const updateParams = useSearchParamUpdater([]);

  const setSort = function (
    sort: AccountingPeriodWithBalanceSort | null,
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

  const currentSort = parseEnumValue(
    AccountingPeriodWithBalanceSort,
    searchParams.get(sortParamName) ?? "",
  );

  const getSortProps = createColumnSortProps(currentSort, setSort);
  const openAccountingPeriod = function (
    accountingPeriod: AccountingPeriodWithBalance,
  ): void {
    router.push(
      routes.workspaceDetail(accountingPeriod.id, {
        years: searchParams.getAll(yearParamName).map(Number),
        months: searchParams.getAll(monthParamName).map(Number),
        ...(currentSort === null ? {} : { sort: currentSort }),
      }),
    );
  };

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
        <Checkbox
          checked={accountingPeriod.isOpen}
          onClick={(event) => {
            event.stopPropagation();
          }}
          slotProps={{
            input: {
              "aria-label": `${accountingPeriod.name} is ${accountingPeriod.isOpen ? "open" : "closed"}`,
              readOnly: true,
              tabIndex: -1,
            },
          }}
        />
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
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (accountingPeriod) => (
        <ListFrameActionButton
          ariaLabel={`View ${accountingPeriod.name} details`}
          onClick={() => {
            openAccountingPeriod(accountingPeriod);
          }}
        >
          <KeyboardArrowRight fontSize="small" color="action" />
        </ListFrameActionButton>
      ),
      alignment: "right",
      minWidth: 52,
      maxWidth: 52,
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
        openAccountingPeriod(accountingPeriod);
      }}
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
                params.delete(actionParamName);
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
