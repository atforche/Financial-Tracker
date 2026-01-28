import type { ApiError } from "@data/ApiError";
import type { Transaction } from "@transactions/ApiTypes";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for deleting a transaction.
 * @param transaction - Transaction to delete.
 */
interface UseDeleteTransactionArgs {
  readonly transaction: Transaction;
}

/**
 * Hook used to delete a Transaction via the API.
 * @param args - Arguments for deleting a Transaction.
 * @returns Running state, success state, current error, and function to delete the Transaction.
 */
const useDeleteTransaction = function ({
  transaction,
}: UseDeleteTransactionArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  error: ApiError | null;
  deleteTransaction: () => void;
} {
  const deleteTransactionCallback = useCallback<
    () => Promise<ApiError | null>
  >(async () => {
    const client = getApiClient();
    const { error } = await client.DELETE("/transactions/{transactionId}", {
      params: {
        path: {
          transactionId: transaction.id,
        },
      },
    });
    return typeof error === "undefined" ? null : error;
  }, [transaction]);

  const { isRunning, isSuccess, error, execute } = useApiRequest({
    apiRequestFunction: deleteTransactionCallback,
  });
  return {
    isRunning,
    isSuccess,
    error,
    deleteTransaction: execute,
  };
};

export default useDeleteTransaction;
