"use client";

import { type Transaction, TransactionSort } from "@/transactions/types";
import {
  getTransactionAccountIds,
  getTransactionFundIds,
} from "@/transactions/postingHelpers";
import { useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriodWithTransactions } from "@/accounting-periods/types";
import { Button } from "@mui/material";
import type { CurrentAccountingPeriodSearchParams } from "@/accounting-periods/current/CurrentAccountingPeriod";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import createTransactionListColumns from "@/transactions/createTransactionListColumns";
import parseEnumValue from "@/framework/data/parseEnumValue";
import propertyName from "@/framework/data/propertyName";
import routes from "@/transactions/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the CurrentAccountingPeriodTransactionListFrame component.
 */
interface CurrentAccountingPeriodTransactionListFrameProps {
  readonly accountingPeriod: AccountingPeriodWithTransactions | null;
}

/**
 * List frame displaying transactions for the current accounting period page.
 */
const CurrentAccountingPeriodTransactionListFrame = function ({
  accountingPeriod,
}: CurrentAccountingPeriodTransactionListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const router = useRouter();

  const sortParamName =
    propertyName<CurrentAccountingPeriodSearchParams>("transactionSort");
  const pageParamName =
    propertyName<CurrentAccountingPeriodSearchParams>("transactionPage");
  const updateParams = useSearchParamUpdater([pageParamName]);

  const setSort = function (sort: TransactionSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const currentSort = parseEnumValue(
    TransactionSort,
    searchParams.get(sortParamName) ?? "",
  );

  const openTransactionWorkspace = function (transaction: Transaction): void {
    router.push(
      routes.workspace({
        accountingPeriodIds: [transaction.accountingPeriodId],
        accountIds: getTransactionAccountIds(transaction),
        fundIds: getTransactionFundIds(transaction),
        selectedTransactionId: transaction.id,
      }),
    );
  };

  const columns = createTransactionListColumns({
    currentSort,
    setSort,
    openTransaction: openTransactionWorkspace,
  });

  return (
    <ListFrame<Transaction>
      title={
        accountingPeriod === null
          ? "Transactions"
          : `Transactions in ${accountingPeriod.name}`
      }
      columns={columns}
      getId={(transaction) => transaction.id}
      data={accountingPeriod?.transactions.items ?? []}
      totalCount={accountingPeriod?.transactions.totalCount ?? 0}
      pageParamName={pageParamName}
      onRowClick={openTransactionWorkspace}
      initialEmptyState={{
        title: "No transactions in this period",
        description:
          "Add or move transactions into this accounting period to see them here.",
        action: (
          <Button
            variant="outlined"
            onClick={() => {
              router.push(routes.workspace({}));
            }}
          >
            Open Transaction Workspace
          </Button>
        ),
      }}
    />
  );
};

export default CurrentAccountingPeriodTransactionListFrame;
