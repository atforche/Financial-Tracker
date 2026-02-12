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
  transactions: Transaction[];
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

export default useGetAccountingPeriodTransactions;
