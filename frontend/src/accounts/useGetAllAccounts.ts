import { type Account, AccountType } from "@accounts/ApiTypes";
import type { ApiError } from "@data/ApiError";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Hook used to retrieve all Accounts via the API.
 * @returns Retrieved Accounts, loading state, current error, and function to refetch the Accounts.
 */
const useGetAllAccounts = function (): {
  accounts: Account[];
  isLoading: boolean;
  error: ApiError | null;
  refetch: () => void;
} {
  const getAllAccountsCallback = useCallback<
    () => Promise<Account[] | ApiError>
  >(async () => {
    const client = getApiClient();
    const { data, error } = await client.GET("/accounts");
    if (typeof error !== "undefined") {
      return error;
    }
    return data;
  }, []);

  const loadingRowCount = 10;
  const { data, isLoading, error, refetch } = useQuery<Account[]>({
    queryFunction: getAllAccountsCallback,
    initialData: Array(loadingRowCount)
      .fill(null)
      .map((_, index) => ({
        id: index.toString(),
        name: "",
        type: AccountType.Standard,
        currentBalance: {
          accountId: "",
          balance: 0,
          fundBalances: [],
          pendingDebitAmount: 0,
          pendingDebits: [],
          pendingCreditAmount: 0,
          pendingCredits: [],
        },
      })),
  });
  return {
    accounts: data,
    isLoading,
    error,
    refetch,
  };
};

export default useGetAllAccounts;
