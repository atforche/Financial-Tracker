import type { Account } from "@accounts/ApiTypes";
import type { ApiError } from "@data/ApiError";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for deleting an Account.
 * @param account - Account to delete.
 */
interface UseDeleteAccountArgs {
  readonly account: Account;
}

/**
 * Hook used to delete an Account via the API.
 * @param args - Arguments for deleting an Account.
 * @returns Running state, success state, current error, and function to delete the Account.
 */
const useDeleteAccount = function ({ account }: UseDeleteAccountArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  error: ApiError | null;
  deleteAccount: () => void;
} {
  const deleteAccountCallback = useCallback<
    () => Promise<ApiError | null>
  >(async () => {
    const client = getApiClient();
    const { error } = await client.DELETE("/accounts/{accountId}", {
      params: {
        path: {
          accountId: account.id,
        },
      },
    });
    return typeof error === "undefined" ? null : error;
  }, [account]);

  const { isRunning, isSuccess, error, execute } = useApiRequest({
    apiRequestFunction: deleteAccountCallback,
  });
  return {
    isRunning,
    isSuccess,
    error,
    deleteAccount: execute,
  };
};

export default useDeleteAccount;
