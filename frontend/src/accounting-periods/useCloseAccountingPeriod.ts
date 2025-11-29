import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import type { ApiError } from "@data/ApiError";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for closing an Accounting Period.
 * @param accountingPeriod - Accounting Period to close.
 */
interface UseCloseAccountingPeriodArgs {
  readonly accountingPeriod: AccountingPeriod;
}

/**
 * Hook used to close an Accounting Period via the API.
 * @param args - Arguments for closing an Accounting Period.
 * @returns Running state, success state, current error, and function to close the Accounting Period.
 */
const useCloseAccountingPeriod = function ({
  accountingPeriod,
}: UseCloseAccountingPeriodArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  error: ApiError | null;
  closeAccountingPeriod: () => void;
} {
  const closeAccountingPeriodCallback = useCallback<
    () => Promise<ApiError | null>
  >(async () => {
    const client = getApiClient();
    const { error } = await client.POST(
      "/accounting-periods/{accountingPeriodId}/close",
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
    apiRequestFunction: closeAccountingPeriodCallback,
  });
  return {
    isRunning,
    isSuccess,
    error,
    closeAccountingPeriod: execute,
  };
};

export default useCloseAccountingPeriod;
