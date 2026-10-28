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
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the TransactionWorkspaceListFrame component.
 */
interface TransactionWorkspaceListFrameProps {
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
  readonly hasActiveFilters: boolean;
}

/**
 * Component that displays the top-level transaction ledger.
 */
const TransactionWorkspaceListFrame = function ({
  data,
  hasActiveFilters,
  totalCount,
}: TransactionWorkspaceListFrameProps): JSX.Element {
  const canWrite = useWriteAccess();
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const accountingPeriodIdsParamName =
    propertyName<TransactionWorkspaceSearchParams>("accountingPeriodIds");
  const accountIdsParamName =
    propertyName<TransactionWorkspaceSearchParams>("accountIds");
  const fundIdsParamName =
    propertyName<TransactionWorkspaceSearchParams>("fundIds");
  const locationIdsParamName =
    propertyName<TransactionWorkspaceSearchParams>("locationIds");
  const fundNamesParamName =
    propertyName<TransactionWorkspaceSearchParams>("fundNames");
  const accountTypesParamName =
    propertyName<TransactionWorkspaceSearchParams>("accountTypes");
  const accountNamesParamName =
    propertyName<TransactionWorkspaceSearchParams>("accountNames");
  const transactionTypesParamName =
    propertyName<TransactionWorkspaceSearchParams>("transactionTypes");
  const fullyPostedParamName =
    propertyName<TransactionWorkspaceSearchParams>("fullyPosted");
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

  // Pages opened from this list return to it, and it keeps its own return link
  const listUrl = buildUrl(pathname, new URLSearchParams(searchParams));
  const openTransaction = function (transaction: Transaction): void {
    router.push(routes.workspaceDetail(transaction.id, { returnUrl: listUrl }));
  };

  const currentSort = parseEnumValue(
    TransactionSort,
    searchParams.get(sortParamName) ?? "",
  );
  const createUrl = buildUrl(
    `${pathname}/create`,
    new URLSearchParams({ returnUrl: listUrl }),
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
      headerContentInline
      headerContent={
        !canWrite ? undefined : (
          <Button
            variant="contained"
            onClick={() => {
              router.push(createUrl);
            }}
          >
            Create
          </Button>
        )
      }
      columns={columns}
      getId={(transaction) => transaction.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
      pageParamName={pageParamName}
      onRowClick={openTransaction}
      initialEmptyState={{
        title: "Search for Transactions",
        description:
          "Select at least one filter and click Search to load transactions.",
        action: null,
      }}
      filteredEmptyState={{
        title: "No Transactions Match This Search",
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
                params.delete(locationIdsParamName);
                params.delete(fundNamesParamName);
                params.delete(accountTypesParamName);
                params.delete(accountNamesParamName);
                params.delete(transactionTypesParamName);
                params.delete(fullyPostedParamName);
                params.delete("startDate");
                params.delete("endDate");
                params.delete("startAccountingPeriodId");
                params.delete("endAccountingPeriodId");
                params.delete(pageParamName);
              });
            }}
          >
            Clear search
          </Button>
        ),
      }}
      hasActiveFilters={hasActiveFilters}
    />
  );
};

export default TransactionWorkspaceListFrame;
