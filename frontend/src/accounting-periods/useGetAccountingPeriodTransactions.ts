import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import type { ApiError } from "@data/ApiError";
import type { Transaction } from "@transactions/ApiTypes";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Arguments for the useGetAccountingPeriodTransactions hook.
 */
interface UseGetAccountingPeriodTransactionsArgs {
  accountingPeriod: AccountingPeriod;
}

/**
 * Hook used to retrieve Accounting Period Transactions via the API.
 * @param args - The arguments for the useGetAccountingPeriodTransactions hook.
 * @returns Retrieved Transactions, loading state, current error, and function to refetch the Transactions.
 */
const useGetAccountingPeriodTransactions = function ({
  accountingPeriod,
}: UseGetAccountingPeriodTransactionsArgs): {
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
      "/accounting-periods/{accountingPeriodId}/transactions",
      {
        params: {
          path: {
            accountingPeriodId: accountingPeriod.id,
          },
        },
      },
    );
    if (typeof error !== "undefined") {
      return error;
    }
    return data;
  }, [accountingPeriod.id]);

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

export default useGetAccountingPeriodTransactions;
