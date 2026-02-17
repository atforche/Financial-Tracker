import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import type { ApiError } from "@data/ApiError";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Hook used to retrieve all open Accounting Periods via the API.
 * @returns Retrieved Accounting Periods, loading state, current error, and function to refetch the Accounting Periods.
 */
const useGetAllOpenAccountingPeriods = function (): {
  accountingPeriods: AccountingPeriod[] | null;
  isLoading: boolean;
  error: ApiError | null;
  refetch: () => void;
} {
  const getAllOpenAccountingPeriodsCallback = useCallback<
    () => Promise<AccountingPeriod[] | ApiError>
  >(async () => {
    const client = getApiClient();
    const { data, error } = await client.GET("/accounting-periods/open");
    if (typeof error !== "undefined") {
      return error;
    }
    return data;
  }, []);

  const { data, isLoading, error, refetch } = useQuery<AccountingPeriod[]>({
    queryFunction: getAllOpenAccountingPeriodsCallback,
  });
  return {
    accountingPeriods: data,
    isLoading,
    error,
    refetch,
  };
};

export default useGetAllOpenAccountingPeriods;
