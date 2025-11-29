import type { ApiError } from "@data/ApiError";
import type { Fund } from "@funds/ApiTypes";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for modifying a fund.
 * @param fund - Fund to modify, or null if creating a new Fund.
 * @param fundName - New name of the Fund.
 * @param fundDescription - New description of the Fund.
 */
interface UseModifyFundArgs {
  readonly fund: Fund | null;
  readonly fundName: string;
  readonly fundDescription: string;
}

/**
 * Hook used to create or update a Fund via the API.
 * @param args - Arguments for modifying a Fund.
 * @returns Running state, success state, current error, and function to modify the Fund.
 */
const useModifyFund = function ({
  fund,
  fundName,
  fundDescription,
}: UseModifyFundArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  error: ApiError | null;
  modifyFund: () => void;
} {
  const modifyFundCallback = useCallback<
    () => Promise<ApiError | null>
  >(async () => {
    const client = getApiClient();
    if (fund === null) {
      // Create a new Fund
      const { error } = await client.POST("/funds", {
        body: { name: fundName, description: fundDescription },
      });
      return typeof error === "undefined" ? null : error;
    }
    // Update an existing Fund
    const { error } = await client.POST("/funds/{fundId}", {
      params: {
        path: {
          fundId: fund.id,
        },
      },
      body: { name: fundName, description: fundDescription },
    });
    return typeof error === "undefined" ? null : error;
  }, [fund, fundName, fundDescription]);

  const { isRunning, isSuccess, error, execute } = useApiRequest({
    apiRequestFunction: modifyFundCallback,
  });

  return { isRunning, isSuccess, error, modifyFund: execute };
};

export default useModifyFund;
