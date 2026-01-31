import type { ApiError } from "@data/ApiError";
import type { Fund } from "@funds/ApiTypes";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Hook used to retrieve all Funds via the API.
 * @returns Retrieved Funds, loading state, current error, and function to refetch the Funds.
 */
const useGetAllFunds = function (): {
  funds: Fund[];
  isLoading: boolean;
  error: ApiError | null;
  refetch: () => void;
} {
  const getAllFundsCallback = useCallback<
    () => Promise<Fund[] | ApiError>
  >(async () => {
    const client = getApiClient();
    const { data, error } = await client.GET("/funds");
    if (typeof error !== "undefined") {
      return error;
    }
    return data;
  }, []);

  const loadingRowCount = 10;
  const { data, isLoading, error, refetch } = useQuery<Fund[]>({
    queryFunction: getAllFundsCallback,
    initialData: Array(loadingRowCount)
      .fill(null)
      .map((_, index) => ({
        id: index.toString(),
        name: "",
        description: "",
        currentBalance: 0,
        pendingDebitAmount: 0,
        pendingCreditAmount: 0,
      })),
  });
  return {
    funds: data,
    isLoading,
    error,
    refetch,
  };
};

export default useGetAllFunds;
