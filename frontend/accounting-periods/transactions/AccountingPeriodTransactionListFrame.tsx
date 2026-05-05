"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { AccountingPeriodTransactionSortOrder } from "@/accounting-periods/types";
import type { AccountingPeriodViewSearchParams } from "@/accounting-periods/AccountingPeriodView";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import IconButton from "@/framework/listframe/IconButton";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { Transaction } from "@/transactions/types";
import formatCurrency from "@/framework/formatCurrency";
import nameof from "@/framework/data/nameof";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the AccountingPeriodTransactionListFrame component.
 */
interface AccountingPeriodTransactionListFrameProps {
  readonly accountingPeriodId: string;
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the list of transactions for an accounting period.
 */
const AccountingPeriodTransactionListFrame = function ({
  accountingPeriodId,
  data,
  totalCount,
}: AccountingPeriodTransactionListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();
  const sortSearchParamName =
    nameof<AccountingPeriodViewSearchParams>("transactionSort");

  const setSort = function (
    sort: AccountingPeriodTransactionSortOrder | null,
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
    AccountingPeriodTransactionSortOrder,
    searchParams.get(sortSearchParamName) ?? "",
  );

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction: Transaction) => transaction.date,
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodTransactionSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.DateDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 125,
    },
    {
      name: "location",
      headerContent: "Location",
      getBodyContent: (transaction: Transaction) => transaction.location,
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.Location
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.LocationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.Location);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.LocationDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "debitFrom",
      headerContent: "Debit From",
      getBodyContent: (transaction: Transaction): string => 
      {
        if ("debitAccount" in transaction) {
          return transaction.debitAccount?.accountName ?? "";
        }
        if ("debitFund" in transaction) {
          return transaction.debitFund.fundName;
        }
        return "";
      },
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.DebitFrom
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.DebitFromDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.DebitFrom);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.DebitFromDescending);
        } else {
          setSort(null);
        }
      }
    },
    {
      name: "creditTo",
      headerContent: "Credit To",
      getBodyContent: (transaction: Transaction): string => {
        if ("creditAccount" in transaction) {
          return transaction.creditAccount?.accountName ?? "";
        }
        if ("creditFund" in transaction) {
          return transaction.creditFund.fundName;
        }
        return "";
      },
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.CreditTo
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.CreditToDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.CreditTo);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.CreditToDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction: Transaction) =>
        formatCurrency(transaction.amount),
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.Amount
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.Amount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.AmountDescending);
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
            router.push(routes.create({ accountingPeriodId }));
          }}
        />
      ),
      getBodyContent: (transaction: Transaction) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(
              routes.detail({ id: transaction.id }, { accountingPeriodId }),
            );
          }}
        />
      ),
      alignment: "right",
      minWidth: 90,
    },
  ];

  return (
    <ListFrame<Transaction>
      columns={columns}
      getId={(transaction) => transaction.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
      pageSearchParamName={nameof<AccountingPeriodViewSearchParams>("page")}
    />
  );
};

export default AccountingPeriodTransactionListFrame;
