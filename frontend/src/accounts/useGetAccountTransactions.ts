import type { Account } from "@accounts/ApiTypes";
import type { ApiError } from "@data/ApiError";
import type { Transaction } from "@transactions/ApiTypes";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Arguments for the useGetAccountTransactions hook.
 */
interface UseGetAccountTransactionsArgs {
  account: Account;
}

/**
 * Hook used to retrieve Account Transactions via the API.
 * @param args - The arguments for the useGetAccountTransactions hook.
 * @returns Retrieved Transactions, loading state, current error, and function to refetch the Transactions.
 */
const useGetAccountTransactions = function ({
  account,
}: UseGetAccountTransactionsArgs): {
  transactions: Transaction[] | null;
  isLoading: boolean;
  error: ApiError | null;
  refetch: () => void;
} {
  const getAllTransactionsCallback = useCallback<
    () => Promise<Transaction[] | ApiError>
  >(async () => {
    const client = getApiClient();
    const { data, error } = await client.GET(
      "/accounts/{accountId}/transactions",
      {
        params: {
          path: {
            accountId: account.id,
          },
        },
      },
    );
    if (typeof error !== "undefined") {
      return error;
    }
    return data;
  }, [account.id]);

  const { data, isLoading, error, refetch } = useQuery<Transaction[]>({
    queryFunction: getAllTransactionsCallback,
  });
  return {
    transactions: data,
    isLoading,
    error,
    refetch,
  };
};

export default useGetAccountTransactions;
