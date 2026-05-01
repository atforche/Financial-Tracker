"use client";

import {
  type AccountingPeriod,
  AccountingPeriodSortOrder,
} from "@/accounting-periods/types";
import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Checkbox } from "@mui/material";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import IconButton from "@/framework/listframe/IconButton";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/framework/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the AccountingPeriodListFrame component.
 */
interface AccountingPeriodListFrameProps {
  readonly data: AccountingPeriod[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that provides a list of Accounting Period and makes the basic create, read, update, and delete
 * operations available on them.
 */
const AccountingPeriodListFrame = function ({
  data,
  totalCount,
}: AccountingPeriodListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const setSort = function (sort: AccountingPeriodSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete("sort");
    } else {
      params.set("sort", sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountingPeriodSortOrder,
    searchParams.get("sort") ?? "",
  );
  const columns: ColumnDefinition<AccountingPeriod>[] = [
    {
      name: "period",
      headerContent: "Period",
      getBodyContent: (accountingPeriod: AccountingPeriod) =>
        accountingPeriod.name,
      sortType:
        currentSort === AccountingPeriodSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
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
      getBodyContent: (accountingPeriod: AccountingPeriod) => (
        <Checkbox checked={accountingPeriod.isOpen} />
      ),
      sortType:
        currentSort === AccountingPeriodSortOrder.IsOpen
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodSortOrder.IsOpenDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
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
      getBodyContent: (accountingPeriod: AccountingPeriod) =>
        formatCurrency(accountingPeriod.openingBalance),
      sortType:
        currentSort === AccountingPeriodSortOrder.OpeningBalance
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodSortOrder.OpeningBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
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
      getBodyContent: (accountingPeriod: AccountingPeriod) =>
        formatCurrency(accountingPeriod.closingBalance),
      sortType:
        currentSort === AccountingPeriodSortOrder.ClosingBalance
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodSortOrder.ClosingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
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
    {
      name: "actions",
      headerContent: (
        <IconButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {
            router.push(routes.accountingPeriods.create);
          }}
        />
      ),
      getBodyContent: (accountingPeriod: AccountingPeriod) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(routes.accountingPeriods.detail(accountingPeriod.id));
          }}
        />
      ),
      alignment: "right",
    },
  ];

  return (
    <ListFrame<AccountingPeriod>
      columns={columns}
      getId={(accountingPeriod: AccountingPeriod) => accountingPeriod.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
    />
  );
};

export default AccountingPeriodListFrame;
