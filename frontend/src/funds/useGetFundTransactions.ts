import type { Fund, FundTransactionSortOrder } from "@funds/ApiTypes";
import type { ApiError } from "@data/ApiError";
import type { Transaction } from "@transactions/ApiTypes";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Arguments for the useGetFundTransactions hook.
 */
interface UseGetFundTransactionsArgs {
  readonly fund: Fund;
  readonly sortBy: FundTransactionSortOrder | null;
  readonly page: number;
  readonly rowsPerPage: number;
}

/**
 * Hook used to retrieve Fund Transactions via the API.
 * @param args - The arguments for the useGetFundTransactions hook.
 * @returns Retrieved Transactions, loading state, current error, and function to refetch the Transactions.
 */
const useGetFundTransactions = function ({
  fund,
  sortBy,
  page,
  rowsPerPage,
}: UseGetFundTransactionsArgs): {
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
    const { data, error } = await client.GET("/funds/{fundId}/transactions", {
      params: {
        path: {
          fundId: fund.id,
        },
        query: {
          SortBy: sortBy,
          Limit: rowsPerPage,
          Offset: page * rowsPerPage,
        },
      },
    });
    if (typeof error !== "undefined") {
      return error;
    }
    return data;
  }, [fund.id, sortBy, page, rowsPerPage]);

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

export default useGetFundTransactions;
