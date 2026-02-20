import type { ApiError } from "@data/ApiError";
import type { Fund } from "@funds/ApiTypes";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Arguments for the useGetAllFunds hook.
 */
interface UseGetAllFundsArgs {
  readonly page: number | null;
  readonly rowsPerPage: number | null;
}

/**
 * Hook used to retrieve all Funds via the API.
 * @param args - The arguments for the useGetAllFunds hook.
 * @returns Retrieved Funds, loading state, current error, and function to refetch the Funds.
 */
const useGetAllFunds = function ({ page, rowsPerPage }: UseGetAllFundsArgs): {
  funds: Fund[] | null;
  totalCount: number | null;
  isLoading: boolean;
  error: ApiError | null;
  refetch: () => void;
} {
  const getAllFundsCallback = useCallback<
    () => Promise<{ items: Fund[]; totalCount: number } | ApiError>
  >(async () => {
    const client = getApiClient();
    const { data, error } = await client.GET("/funds", {
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
    items: Fund[];
    totalCount: number;
  }>({
    queryFunction: getAllFundsCallback,
  });
  return {
    funds: data?.items ?? null,
    totalCount: data?.totalCount ?? null,
    isLoading,
    error,
    refetch,
  };
};

export default useGetAllFunds;
