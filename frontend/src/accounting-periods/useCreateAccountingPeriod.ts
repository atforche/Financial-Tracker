import type { ApiError } from "@data/ApiError";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for creating an Accounting Period.
 * @param year - Year for the Accounting Period.
 * @param month - Month for the Accounting Period.
 */
interface UseCreateAccountingPeriodArgs {
  readonly year: number;
  readonly month: number;
}

/**
 * Hook used to create an Accounting Period via the API.
 * @param args - Arguments for creating an Accounting Period.
 * @returns Running state, success state, current error, and function to create the Accounting Period.
 */
const useCreateAccountingPeriod = function ({
  year,
  month,
}: UseCreateAccountingPeriodArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  error: ApiError | null;
  createAccountingPeriod: () => void;
} {
  const createAccountingPeriodCallback = useCallback<
    () => Promise<ApiError | null>
  >(async () => {
    const client = getApiClient();
    const { error } = await client.POST("/accounting-periods", {
      body: { year, month },
    });
    return typeof error === "undefined" ? null : error;
  }, [year, month]);

  const { isRunning, isSuccess, error, execute } = useApiRequest({
    apiRequestFunction: createAccountingPeriodCallback,
  });

  return { isRunning, isSuccess, error, createAccountingPeriod: execute };
};

export default useCreateAccountingPeriod;
