"use client";

import { type Transaction, TransactionSort } from "@/transactions/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Button } from "@mui/material";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import { buildUrl } from "@/framework/routes/helpers";
import createTransactionListColumns from "@/transactions/createTransactionListColumns";
import { normalizeAccountTypes } from "@/accounts/accountTypeFilterHelpers";
import { normalizeTransactionTypes } from "@/transactions/transactionTypeFilter";
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
}

/**
 * Component that displays the top-level transaction ledger.
 */
const TransactionWorkspaceListFrame = function ({
  data,
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
  const accountTypesParamName =
    propertyName<TransactionWorkspaceSearchParams>("accountTypes");
  const accountNamesParamName =
    propertyName<TransactionWorkspaceSearchParams>("accountNames");
  const transactionTypesParamName =
    propertyName<TransactionWorkspaceSearchParams>("transactionTypes");
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
    const startDate = params.get("startDate");
    const endDate = params.get("endDate");
    const startAccountingPeriodId = params.get("startAccountingPeriodId");
    const endAccountingPeriodId = params.get("endAccountingPeriodId");
    const pageSize = params.get("pageSize");
    const returnUrl = params.get("returnUrl");
    router.push(
      routes.workspaceDetail(transaction.id, {
        accountingPeriodIds: params.getAll(accountingPeriodIdsParamName),
        accountIds: params.getAll(accountIdsParamName),
        fundIds: params.getAll(fundIdsParamName),
        accountTypes: normalizeAccountTypes(
          params.getAll(accountTypesParamName),
        ),
        accountNames: params.getAll(accountNamesParamName),
        transactionTypes: normalizeTransactionTypes(
          params.getAll(transactionTypesParamName),
        ),
        ...(startDate === null ? {} : { startDate }),
        ...(endDate === null ? {} : { endDate }),
        ...(startAccountingPeriodId === null
          ? {}
          : { startAccountingPeriodId }),
        ...(endAccountingPeriodId === null ? {} : { endAccountingPeriodId }),
        sort:
          parseEnumValue(TransactionSort, params.get(sortParamName) ?? "") ??
          null,
        page: params.get(pageParamName),
        ...(pageSize === null ? {} : { pageSize }),
        ...(returnUrl === null ? {} : { returnUrl }),
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
        !canWrite ? undefined : (
          <Button
            variant="contained"
            onClick={() => {
              router.push(createUrl);
            }}
          >
            Create Transaction
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
        title: "No Transactions Found",
        description: "No transactions have been recorded yet.",
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
                params.delete(accountTypesParamName);
                params.delete(accountNamesParamName);
                params.delete(transactionTypesParamName);
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
    />
  );
};

export default TransactionWorkspaceListFrame;
