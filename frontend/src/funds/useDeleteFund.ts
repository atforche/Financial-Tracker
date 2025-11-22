import type { ApiError } from "@data/ApiError";
import type { Fund } from "@funds/ApiTypes";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for deleting a fund.
 * @param {Fund} fund - Fund to delete.
 */
interface UseDeleteFundArgs {
  readonly fund: Fund;
}

/**
 * Hook used to delete a Fund via the API.
 * @param {UseDeleteFundArgs} args - Arguments for deleting a Fund.
 * @returns {{isRunning: boolean, isSuccess: boolean, error: ApiError | null, deleteFund: () => void}} - Running state, success state, current error, and function to delete the Fund.
 */
const useDeleteFund = function ({ fund }: UseDeleteFundArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  error: ApiError | null;
  deleteFund: () => void;
} {
  const deleteFundCallback = useCallback<
    () => Promise<ApiError | null>
  >(async () => {
    const client = getApiClient();
    const { error } = await client.DELETE("/funds/{fundId}", {
      params: {
        path: {
          fundId: fund.id,
        },
      },
    });
    return typeof error === "undefined" ? null : error;
  }, [fund]);

  const { isRunning, isSuccess, error, execute } = useApiRequest({
    apiRequestFunction: deleteFundCallback,
  });
  return {
    isRunning,
    isSuccess,
    error,
    deleteFund: execute,
  };
};

export default useDeleteFund;
