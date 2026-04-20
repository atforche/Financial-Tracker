"use client";

import { type Account, AccountSortOrder } from "@/data/accountTypes";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import IconButton from "@/framework/listframe/IconButton";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/framework/routes";
import tryParseEnum from "@/data/tryParseEnum";

/**
 * Props for the AccountListFrame component.
 */
interface AccountListFrameProps {
  readonly data: Account[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the list of accounts associated with an accounting period.
 */
const AccountListFrame = function ({
  data,
  totalCount,
}: AccountListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const setSort = function (sort: AccountSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete("sort");
    } else {
      params.set("sort", sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountSortOrder,
    searchParams.get("sort") ?? "",
  );

  const columns: ColumnDefinition<Account>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (account: Account) => account.name,
      sortType:
        currentSort === AccountSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === AccountSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountSortOrder.NameDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "type",
      headerContent: "Type",
      getBodyContent: (account: Account) => account.type,
      sortType:
        currentSort === AccountSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === AccountSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountSortOrder.TypeDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "postedBalance",
      headerContent: "Posted Balance",
      getBodyContent: (account: Account) =>
        formatCurrency(account.currentBalance.postedBalance),
      sortType:
        currentSort === AccountSortOrder.PostedBalance
          ? ColumnSortType.Ascending
          : currentSort === AccountSortOrder.PostedBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountSortOrder.PostedBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountSortOrder.PostedBalanceDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 125,
      alignment: "right",
    },
    {
      name: "availableToSpend",
      headerContent: "Available to Spend",
      getBodyContent: (account: Account) =>
        account.currentBalance.availableToSpend !== null
          ? formatCurrency(account.currentBalance.availableToSpend)
          : null,
      sortType:
        currentSort === AccountSortOrder.AvailableToSpend
          ? ColumnSortType.Ascending
          : currentSort === AccountSortOrder.AvailableToSpendDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountSortOrder.AvailableToSpend);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountSortOrder.AvailableToSpendDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 125,
      alignment: "right",
    },
    {
      name: "actions",
      headerContent: (
        <IconButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {
            router.push(routes.accounts.create);
          }}
        />
      ),
      getBodyContent: () => null,
      alignment: "right",
      minWidth: 60,
    },
  ];

  return (
    <ListFrame<Account>
      columns={columns}
      getId={(account) => account.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
    />
  );
};

export default AccountListFrame;
