import type { ApiError } from "@data/ApiError";
import type { Fund } from "@funds/ApiTypes";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for creating of updating a Fund.
 */
interface UseCreateOrUpdateFundArgs {
  readonly fund: Fund | null;
  readonly fundName: string;
  readonly fundDescription: string;
}

/**
 * Hook used to create or update a Fund via the API.
 * @param args - Arguments for creating or updating a Fund.
 * @returns Running state, success state, current error, and function to modify the Fund.
 */
const useCreateOrUpdateFund = function ({
  fund,
  fundName,
  fundDescription,
}: UseCreateOrUpdateFundArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  createdOrUpdatedFund: Fund | null;
  error: ApiError | null;
  modifyFund: () => void;
} {
  const modifyFundCallback = useCallback<
    () => Promise<Fund | ApiError | null>
  >(async () => {
    const client = getApiClient();
    if (fund === null) {
      // Create a new Fund
      const { data, error } = await client.POST("/funds", {
        body: { name: fundName, description: fundDescription },
      });
      return typeof error === "undefined" ? data : error;
    }
    // Update an existing Fund
    const { data, error } = await client.POST("/funds/{fundId}", {
      params: {
        path: {
          fundId: fund.id,
        },
      },
      body: { name: fundName, description: fundDescription },
    });
    return typeof error === "undefined" ? data : error;
  }, [fund, fundName, fundDescription]);

  const {
    isRunning,
    isSuccess,
    response: createdOrUpdatedFund,
    error,
    execute,
  } = useApiRequest<Fund>({
    apiRequestFunction: modifyFundCallback,
  });

  return {
    isRunning,
    isSuccess,
    createdOrUpdatedFund,
    error,
    modifyFund: execute,
  };
};

export default useCreateOrUpdateFund;
