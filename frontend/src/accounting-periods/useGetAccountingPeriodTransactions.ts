import type {
  AccountingPeriod,
  AccountingPeriodTransactionSortOrder,
} from "@accounting-periods/ApiTypes";
import type { ApiError } from "@data/ApiError";
import type { Transaction } from "@transactions/ApiTypes";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Arguments for the useGetAccountingPeriodTransactions hook.
 */
interface UseGetAccountingPeriodTransactionsArgs {
  readonly accountingPeriod: AccountingPeriod;
  readonly sortBy: AccountingPeriodTransactionSortOrder | null;
  readonly page: number;
  readonly rowsPerPage: number;
}

/**
 * Hook used to retrieve Accounting Period Transactions via the API.
 * @param args - The arguments for the useGetAccountingPeriodTransactions hook.
 * @returns Retrieved Transactions, loading state, current error, and function to refetch the Transactions.
 */
const useGetAccountingPeriodTransactions = function ({
  accountingPeriod,
  sortBy,
  page,
  rowsPerPage,
}: UseGetAccountingPeriodTransactionsArgs): {
  transactions: Transaction[] | null;
  totalCount: number | null;
  isLoading: boolean;
  error: ApiError | null;
  refetch: () => void;
} {
  const getAllTransactionsCallback = useCallback<
    () => Promise<{ items: Transaction[]; totalCount: number } | ApiError>
  >(async () => {
    const client = getApiClient();
    const { data, error } = await client.GET(
      "/accounting-periods/{accountingPeriodId}/transactions",
      {
        params: {
          path: {
            accountingPeriodId: accountingPeriod.id,
          },
          query: {
            SortBy: sortBy,
            Limit: rowsPerPage,
            Offset: page * rowsPerPage,
          },
        },
      },
    );
    if (typeof error !== "undefined") {
      return error;
    }
    return data;
  }, [accountingPeriod.id, sortBy, page, rowsPerPage]);

  const { data, isLoading, error, refetch } = useQuery<{
    items: Transaction[];
    totalCount: number;
  }>({
    queryFunction: getAllTransactionsCallback,
  });
  return {
    transactions: data?.items ?? null,
    totalCount: data?.totalCount ?? null,
    isLoading,
    error,
    refetch,
  };
};

export default useGetAccountingPeriodTransactions;
