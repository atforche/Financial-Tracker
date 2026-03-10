"use client";

import {
  type AccountingPeriod,
  AccountingPeriodSortOrder,
} from "@/data/accountingPeriodTypes";
import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Checkbox } from "@mui/material";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnHeaderButton from "@/framework/listframe/ColumnHeaderButton";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import tryParseEnum from "@/data/tryParseEnum";

/**
 * Props for the AccountingPeriodListFrame component.
 */
interface AccountingPeriodListFrameProps {
  readonly data: AccountingPeriod[] | null;
}

/**
 * Component that provides a list of Accounting Period and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns JSX element representing a list of Accounting Period with various action buttons.
 */
const AccountingPeriodListFrame = function ({
  data,
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
      name: "actions",
      headerContent: (
        <ColumnHeaderButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {}}
        />
      ),
      getBodyContent: (accountingPeriod: AccountingPeriod) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {}}
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
    />
  );
};

export default AccountingPeriodListFrame;
