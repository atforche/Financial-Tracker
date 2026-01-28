import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import type { ApiError } from "@data/ApiError";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Hook used to retrieve all Accounting Periods via the API.
 * @returns Retrieved Accounting Periods, loading state, current error, and function to refetch the Accounting Periods.
 */
const useGetAllAccountingPeriods = function (): {
  accountingPeriods: AccountingPeriod[];
  isLoading: boolean;
  error: ApiError | null;
  refetch: () => void;
} {
  const getAllAccountingPeriodsCallback = useCallback<
    () => Promise<AccountingPeriod[] | ApiError>
  >(async () => {
    const client = getApiClient();
    const { data, error } = await client.GET("/accounting-periods");
    if (typeof error !== "undefined") {
      return error;
    }
    return data;
  }, []);

  const loadingRowCount = 10;
  const { data, isLoading, error, refetch } = useQuery<AccountingPeriod[]>({
    queryFunction: getAllAccountingPeriodsCallback,
    initialData: Array(loadingRowCount)
      .fill(null)
      .map((_, index) => ({
        id: index.toString(),
        name: "",
        year: 0,
        month: 0,
        isOpen: false,
      })),
  });
  return {
    accountingPeriods: data,
    isLoading,
    error,
    refetch,
  };
};

export default useGetAllAccountingPeriods;
