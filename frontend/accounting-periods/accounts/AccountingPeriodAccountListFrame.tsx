"use client";

import {
  type AccountingPeriod,
  type AccountingPeriodAccount,
  AccountingPeriodAccountSortOrder,
} from "@/accounting-periods/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriodDetailViewSearchParams } from "@/accounting-periods/detail/AccountingPeriodDetailView";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import IconButton from "@/framework/listframe/IconButton";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import accountRoutes from "@/accounts/routes";
import formatCurrency from "@/framework/formatCurrency";
import nameof from "@/framework/data/nameof";
import routes from "@/accounting-periods/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the AccountingPeriodAccountListFrame component.
 */
interface AccountingPeriodAccountListFrameProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly data: AccountingPeriodAccount[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the list of accounts associated with an accounting period.
 */
const AccountingPeriodAccountListFrame = function ({
  accountingPeriod,
  data,
  totalCount,
}: AccountingPeriodAccountListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();
  const sortSearchParamName =
    nameof<AccountingPeriodDetailViewSearchParams>("accountSort");

  const setSort = function (
    sort: AccountingPeriodAccountSortOrder | null,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortSearchParamName);
    } else {
      params.set(sortSearchParamName, sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountingPeriodAccountSortOrder,
    searchParams.get(sortSearchParamName) ?? "",
  );

  const columns: ColumnDefinition<AccountingPeriodAccount>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (account: AccountingPeriodAccount) => account.name,
      sortType:
        currentSort === AccountingPeriodAccountSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodAccountSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodAccountSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodAccountSortOrder.NameDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "type",
      headerContent: "Type",
      getBodyContent: (account: AccountingPeriodAccount) => account.type,
      sortType:
        currentSort === AccountingPeriodAccountSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodAccountSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodAccountSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodAccountSortOrder.TypeDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "openingBalance",
      headerContent: "Opening Balance",
      getBodyContent: (account: AccountingPeriodAccount) =>
        formatCurrency(account.openingBalance),
      sortType:
        currentSort === AccountingPeriodAccountSortOrder.OpeningBalance
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodAccountSortOrder.OpeningBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodAccountSortOrder.OpeningBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodAccountSortOrder.OpeningBalanceDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 125,
      alignment: "right",
    },
    {
      name: "closingBalance",
      headerContent: "Closing Balance",
      getBodyContent: (account: AccountingPeriodAccount) =>
        formatCurrency(account.closingBalance),
      sortType:
        currentSort === AccountingPeriodAccountSortOrder.ClosingBalance
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodAccountSortOrder.ClosingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodAccountSortOrder.ClosingBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodAccountSortOrder.ClosingBalanceDescending);
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
            router.push(
              accountRoutes.create({
                accountingPeriodId: accountingPeriod.id,
              }),
            );
          }}
          disabled={!accountingPeriod.isOpen}
        />
      ),
      getBodyContent: (account: AccountingPeriodAccount) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(
              routes.accountDetail({
                id: accountingPeriod.id,
                accountId: account.id,
              }),
            );
          }}
        />
      ),
      alignment: "right",
      minWidth: 90,
    },
  ];

  return (
    <ListFrame<AccountingPeriodAccount>
      columns={columns}
      getId={(account) => account.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
      pageSearchParamName={nameof<AccountingPeriodDetailViewSearchParams>(
        "page",
      )}
    />
  );
};

export default AccountingPeriodAccountListFrame;
