"use client";

import { type Transaction, TransactionSort } from "@/transactions/types";
import {
  getTransactionAccountIds,
  getTransactionFundIds,
} from "@/transactions/postingHelpers";
import { useRouter, useSearchParams } from "next/navigation";
import parseEnumValue from "@/framework/data/parseEnumValue";
import routes from "@/transactions/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Shared sorting and workspace navigation behavior for transaction lists.
 */
const useTransactionList = function (
  sortParamName: string,
  pageParamName: string,
): {
  readonly currentSort: TransactionSort | null;
  readonly openTransactionWorkspace: (transaction: Transaction) => void;
  readonly setSort: (sort: TransactionSort | null) => void;
} {
  const router = useRouter();
  const searchParams = useSearchParams();
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

  return {
    currentSort: parseEnumValue(
      TransactionSort,
      searchParams.get(sortParamName) ?? "",
    ),
    openTransactionWorkspace,
    setSort,
  };
};

export default useTransactionList;
