"use client";

import {
  type AccountingPeriod,
  type AccountingPeriodFund,
  AccountingPeriodFundSortOrder,
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
import formatCurrency from "@/framework/formatCurrency";
import fundRoutes from "@/funds/routes";
import nameof from "@/framework/data/nameof";
import routes from "@/accounting-periods/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the AccountingPeriodFundListFrame component.
 */
interface AccountingPeriodFundListFrameProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly data: AccountingPeriodFund[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the list of funds associated with an accounting period.
 */
const AccountingPeriodFundListFrame = function ({
  accountingPeriod,
  data,
  totalCount,
}: AccountingPeriodFundListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();
  const sortSearchParamName =
    nameof<AccountingPeriodDetailViewSearchParams>("fundSort");

  const setSort = function (sort: AccountingPeriodFundSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortSearchParamName);
    } else {
      params.set(sortSearchParamName, sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountingPeriodFundSortOrder,
    searchParams.get(sortSearchParamName) ?? "",
  );

  const columns: ColumnDefinition<AccountingPeriodFund>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (fund: AccountingPeriodFund) => fund.name,
      sortType:
        currentSort === AccountingPeriodFundSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodFundSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.NameDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "openingBalance",
      headerContent: "Opening Balance",
      getBodyContent: (fund: AccountingPeriodFund) =>
        formatCurrency(fund.openingBalance),
      sortType:
        currentSort === AccountingPeriodFundSortOrder.OpeningBalance
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundSortOrder.OpeningBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.OpeningBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.OpeningBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "amountAssigned",
      headerContent: "Amount Assigned",
      getBodyContent: (fund: AccountingPeriodFund) =>
        formatCurrency(fund.amountAssigned),
      sortType:
        currentSort === AccountingPeriodFundSortOrder.AmountAssigned
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundSortOrder.AmountAssignedDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.AmountAssigned);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.AmountAssignedDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "amountSpent",
      headerContent: "Amount Spent",
      getBodyContent: (fund: AccountingPeriodFund) =>
        formatCurrency(fund.amountSpent),
      sortType:
        currentSort === AccountingPeriodFundSortOrder.AmountSpent
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodFundSortOrder.AmountSpentDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.AmountSpent);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.AmountSpentDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "closingBalance",
      headerContent: "Closing Balance",
      getBodyContent: (fund: AccountingPeriodFund) =>
        formatCurrency(fund.closingBalance),
      sortType:
        currentSort === AccountingPeriodFundSortOrder.ClosingBalance
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundSortOrder.ClosingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.ClosingBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.ClosingBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "actions",
      headerContent: (
        <IconButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {
            router.push(
              fundRoutes.create({ accountingPeriodId: accountingPeriod.id }),
            );
          }}
          disabled={!accountingPeriod.isOpen}
        />
      ),
      getBodyContent: (fund: AccountingPeriodFund) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(
              routes.fundDetail(
                { id: accountingPeriod.id, fundId: fund.id },
                {},
              ),
            );
          }}
        />
      ),
      alignment: "right",
      minWidth: 90,
    },
  ];

  return (
    <ListFrame<AccountingPeriodFund>
      columns={columns}
      getId={(fund) => fund.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
      pageSearchParamName={nameof<AccountingPeriodDetailViewSearchParams>(
        "page",
      )}
    />
  );
};

export default AccountingPeriodFundListFrame;
