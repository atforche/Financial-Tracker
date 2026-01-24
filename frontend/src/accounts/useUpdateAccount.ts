import type { Account } from "@accounts/ApiTypes";
import type { ApiError } from "@data/ApiError";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for updating an Account.
 * @param account - Account to update.
 * @param name - New name for the Account.
 */
interface UseCloseAccountingPeriodArgs {
  readonly account: Account;
  readonly name: string;
}

/**
 * Hook used to update an Account via the API.
 * @param args - Arguments for updating an Account.
 * @returns Running state, success state, current error, and function to update the Account.
 */
const useUpdateAccount = function ({
  account,
  name,
}: UseCloseAccountingPeriodArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  error: ApiError | null;
  updateAccount: () => void;
} {
  const updateAccountCallback = useCallback<
    () => Promise<ApiError | null>
  >(async () => {
    const client = getApiClient();
    const { error } = await client.POST("/accounts/{accountId}", {
      params: {
        path: {
          accountId: account.id,
        },
      },
      body: { name },
    });
    return typeof error === "undefined" ? null : error;
  }, [account, name]);

  const { isRunning, isSuccess, error, execute } = useApiRequest({
    apiRequestFunction: updateAccountCallback,
  });
  return {
    isRunning,
    isSuccess,
    error,
    updateAccount: execute,
  };
};

export default useUpdateAccount;
