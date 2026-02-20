import type { Account } from "@accounts/ApiTypes";
import type { ApiError } from "@data/ApiError";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Arguments for the useGetAllAccounts hook.
 */
interface UseGetAllAccountsArgs {
  readonly page: number | null;
  readonly rowsPerPage: number | null;
}

/**
 * Hook used to retrieve all Accounts via the API.
 * @param args - The arguments for the useGetAllAccounts hook.
 * @returns Retrieved Accounts, loading state, current error, and function to refetch the Accounts.
 */
const useGetAllAccounts = function ({
  page,
  rowsPerPage,
}: UseGetAllAccountsArgs): {
  accounts: Account[] | null;
  totalCount: number | null;
  isLoading: boolean;
  error: ApiError | null;
  refetch: () => void;
} {
  const getAllAccountsCallback = useCallback<
    () => Promise<{ items: Account[]; totalCount: number } | ApiError>
  >(async () => {
    const client = getApiClient();
    const { data, error } = await client.GET("/accounts", {
      params: {
        query:
          page !== null && rowsPerPage !== null
            ? { Limit: rowsPerPage, Offset: page * rowsPerPage }
            : {},
      },
    });
    if (typeof error !== "undefined") {
      return error;
    }
    return data;
  }, [page, rowsPerPage]);

  const { data, isLoading, error, refetch } = useQuery<{
    items: Account[];
    totalCount: number;
  }>({
    queryFunction: getAllAccountsCallback,
  });
  return {
    accounts: data?.items ?? null,
    totalCount: data?.totalCount ?? null,
    isLoading,
    error,
    refetch,
  };
};

export default useGetAllAccounts;
