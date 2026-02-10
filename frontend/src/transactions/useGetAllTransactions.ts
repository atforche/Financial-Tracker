import type { ApiError } from "@data/ApiError";
import type { Transaction } from "@transactions/ApiTypes";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Hook used to retrieve all Transactions via the API.
 * @returns Retrieved Transactions, loading state, current error, and function to refetch the Transactions.
 */
const useGetAllTransactions = function (): {
  transactions: Transaction[];
  isLoading: boolean;
  error: ApiError | null;
  refetch: () => void;
} {
  const getAllTransactionsCallback = useCallback<
    () => Promise<Transaction[] | ApiError>
  >(async () => {
    const client = getApiClient();
    const { data, error } = await client.GET("/transactions");
    if (typeof error !== "undefined") {
      return error;
    }
    return data;
  }, []);

  const loadingRowCount = 10;
  const { data, isLoading, error, refetch } = useQuery<Transaction[]>({
    queryFunction: getAllTransactionsCallback,
    initialData: Array(loadingRowCount)
      .fill(null)
      .map((_, index) => ({
        id: index.toString(),
        accountingPeriodId: "",
        accountingPeriodName: "",
        date: "",
        location: "",
        description: "",
        amount: 0,
        debitAccount: {
          accountId: "",
          accountName: "",
          postedDate: null,
          fundAmounts: [],
          previousAccountBalance: {
            accountId: "",
            balance: 0,
            fundBalances: [],
            pendingDebitAmount: 0,
            pendingDebits: [],
            pendingCreditAmount: 0,
            pendingCredits: [],
          },
          newAccountBalance: {
            accountId: "",
            accountName: "",
            balance: 0,
            fundBalances: [],
            pendingDebitAmount: 0,
            pendingDebits: [],
            pendingCreditAmount: 0,
            pendingCredits: [],
          },
        },
        creditAccount: {
          accountId: "",
          accountName: "",
          postedDate: null,
          fundAmounts: [],
          previousAccountBalance: {
            accountId: "",
            balance: 0,
            fundBalances: [],
            pendingDebitAmount: 0,
            pendingDebits: [],
            pendingCreditAmount: 0,
            pendingCredits: [],
          },
          newAccountBalance: {
            accountId: "",
            accountName: "",
            balance: 0,
            fundBalances: [],
            pendingDebitAmount: 0,
            pendingDebits: [],
            pendingCreditAmount: 0,
            pendingCredits: [],
          },
        },
        previousFundBalances: [],
        newFundBalances: [],
      })),
  });
  return {
    transactions: data,
    isLoading,
    error,
    refetch,
  };
};

export default useGetAllTransactions;
