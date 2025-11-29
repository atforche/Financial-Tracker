import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import type { ApiError } from "@data/ApiError";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for deleting an accounting period.
 * @param accountingPeriod - Accounting Period to delete.
 */
interface UseDeleteAccountingPeriodArgs {
  readonly accountingPeriod: AccountingPeriod;
}

/**
 * Hook used to delete an Accounting Period via the API.
 * @param args - Arguments for deleting an Accounting Period.
 * @returns Running state, success state, current error, and function to delete the Accounting Period.
 */
const useDeleteAccountingPeriod = function ({
  accountingPeriod,
}: UseDeleteAccountingPeriodArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  error: ApiError | null;
  deleteAccountingPeriod: () => void;
} {
  const deleteAccountingPeriodCallback = useCallback<
    () => Promise<ApiError | null>
  >(async () => {
    const client = getApiClient();
    const { error } = await client.DELETE(
      "/accounting-periods/{accountingPeriodId}",
      {
        params: {
          path: {
            accountingPeriodId: accountingPeriod.id,
          },
        },
      },
    );
    return typeof error === "undefined" ? null : error;
  }, [accountingPeriod]);

  const { isRunning, isSuccess, error, execute } = useApiRequest({
    apiRequestFunction: deleteAccountingPeriodCallback,
  });
  return {
    isRunning,
    isSuccess,
    error,
    deleteAccountingPeriod: execute,
  };
};

export default useDeleteAccountingPeriod;
