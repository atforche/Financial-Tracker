"use client";

import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { Transaction } from "@/transactions/types";
import createTransactionListColumns from "@/transactions/createTransactionListColumns";
import useTransactionList from "@/transactions/useTransactionList";

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
  const { currentSort, openTransactionWorkspace, setSort } = useTransactionList(
    sortParamName,
    pageParamName,
  );

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
