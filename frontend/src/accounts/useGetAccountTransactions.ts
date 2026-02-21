import type { Account, AccountTransactionSortOrder } from "@accounts/ApiTypes";
import type { ApiError } from "@data/ApiError";
import type { Transaction } from "@transactions/ApiTypes";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Arguments for the useGetAccountTransactions hook.
 */
interface UseGetAccountTransactionsArgs {
  readonly account: Account;
  readonly sortBy: AccountTransactionSortOrder | null;
  readonly page: number;
  readonly rowsPerPage: number;
}

/**
 * Hook used to retrieve Account Transactions via the API.
 * @param args - The arguments for the useGetAccountTransactions hook.
 * @returns Retrieved Transactions, loading state, current error, and function to refetch the Transactions.
 */
const useGetAccountTransactions = function ({
  account,
  sortBy,
  page,
  rowsPerPage,
}: UseGetAccountTransactionsArgs): {
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
      "/accounts/{accountId}/transactions",
      {
        params: {
          path: {
            accountId: account.id,
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
  }, [account.id, sortBy, page, rowsPerPage]);

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

export default useGetAccountTransactions;
