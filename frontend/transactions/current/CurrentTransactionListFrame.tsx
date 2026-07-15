"use client";

import { type Transaction, TransactionSort } from "@/transactions/types";
import {
  getTransactionAccountIds,
  getTransactionFundIds,
} from "@/transactions/postingHelpers";
import { useRouter, useSearchParams } from "next/navigation";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import createTransactionListColumns from "@/transactions/createTransactionListColumns";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the CurrentTransactionListFrame component.
 */
interface CurrentTransactionListFrameProps {
  readonly title: string;
  readonly data: Transaction[];
  readonly totalCount: number;
  readonly sortParamName: string;
  readonly pageParamName: string;
  readonly searchParamName: string;
  readonly emptyTitle: string;
  readonly emptyDescription: string;
  readonly emptyAction?: JSX.Element | null;
}

/**
 * List frame that displays transactions for the current page.
 */
const CurrentTransactionListFrame = function ({
  title,
  data,
  totalCount,
  sortParamName,
  pageParamName,
  searchParamName,
  emptyTitle,
  emptyDescription,
  emptyAction,
}: CurrentTransactionListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const router = useRouter();

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

  const currentSort = tryParseEnum(
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
      title={title}
      columns={columns}
      getId={(transaction) => transaction.id}
      data={data}
      totalCount={totalCount}
      pageParamName={pageParamName}
      searchParamName={searchParamName}
      onRowClick={openTransactionWorkspace}
      initialEmptyState={{
        title: emptyTitle,
        description: emptyDescription,
        action: emptyAction ?? null,
      }}
    />
  );
};

export default CurrentTransactionListFrame;
