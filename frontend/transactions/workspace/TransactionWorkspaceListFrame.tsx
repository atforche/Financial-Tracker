"use client";

import { type Transaction, TransactionSort } from "@/transactions/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Button } from "@mui/material";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import { buildUrl } from "@/framework/routes/helpers";
import createTransactionListColumns from "@/transactions/createTransactionListColumns";
import parseEnumValue from "@/framework/data/parseEnumValue";
import propertyName from "@/framework/data/propertyName";
import routes from "@/transactions/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the TransactionWorkspaceListFrame component.
 */
interface TransactionWorkspaceListFrameProps {
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the top-level transaction ledger.
 */
const TransactionWorkspaceListFrame = function ({
  data,
  totalCount,
}: TransactionWorkspaceListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const accountingPeriodIdsParamName =
    propertyName<TransactionWorkspaceSearchParams>("accountingPeriodIds");
  const accountIdsParamName =
    propertyName<TransactionWorkspaceSearchParams>("accountIds");
  const fundIdsParamName =
    propertyName<TransactionWorkspaceSearchParams>("fundIds");
  const sortParamName = propertyName<TransactionWorkspaceSearchParams>("sort");
  const pageParamName = propertyName<TransactionWorkspaceSearchParams>("page");

  const updateParams = useSearchParamUpdater([]);

  const setSort = function (sort: TransactionSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
      params.delete(pageParamName);
    });
  };

  const openTransaction = function (transaction: Transaction): void {
    const params = new URLSearchParams(searchParams.toString());
    router.push(
      routes.workspaceDetail(transaction.id, {
        accountingPeriodIds: params.getAll(accountingPeriodIdsParamName),
        accountIds: params.getAll(accountIdsParamName),
        fundIds: params.getAll(fundIdsParamName),
        sort:
          parseEnumValue(TransactionSort, params.get(sortParamName) ?? "") ??
          null,
        page: params.get(pageParamName),
      } satisfies TransactionWorkspaceSearchParams),
      { scroll: false },
    );
  };

  const currentSort = parseEnumValue(
    TransactionSort,
    searchParams.get(sortParamName) ?? "",
  );
  const createUrl = buildUrl(
    `${pathname}/create`,
    new URLSearchParams(searchParams),
  );

  const columns = createTransactionListColumns({
    currentSort,
    openTransaction,
    setSort,
    includeFullyPosted: true,
  });

  return (
    <ListFrame<Transaction>
      title="Transactions"
      headerContent={
        <Button
          variant="contained"
          onClick={() => {
            router.push(createUrl);
          }}
        >
          Create Transaction
        </Button>
      }
      columns={columns}
      getId={(transaction) => transaction.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
      pageParamName={pageParamName}
      onRowClick={openTransaction}
      initialEmptyState={{
        title: "No transactions found",
        description: "No transactions have been recorded yet.",
        action: null,
      }}
      filteredEmptyState={{
        title: "No transactions match this search",
        description:
          "Try a different description, amount, date, or account name, or clear the current search to see all matching transactions.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              updateParams((params) => {
                params.delete(accountingPeriodIdsParamName);
                params.delete(accountIdsParamName);
                params.delete(fundIdsParamName);
                params.delete(pageParamName);
              });
            }}
          >
            Clear search
          </Button>
        ),
      }}
    />
  );
};

export default TransactionWorkspaceListFrame;
