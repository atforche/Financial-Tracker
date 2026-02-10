import type { ApiError } from "@data/ApiError";
import type { Dayjs } from "dayjs";
import type { FundAmount } from "@funds/ApiTypes";
import type { Transaction } from "@transactions/ApiTypes";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for updating a Transaction.
 */
interface UseUpdateTransactionArgs {
  readonly existingTransaction: Transaction;
  readonly date: Dayjs | null;
  readonly location: string;
  readonly description: string;
  readonly debitFundAmounts: FundAmount[];
  readonly creditFundAmounts: FundAmount[];
}

/**
 * Hook used to update a Transaction via the API.
 * @param args - Arguments for updating a Transaction.
 * @returns Running state, success state, current error, and function to update the Transaction.
 */
const useUpdateTransaction = function ({
  existingTransaction,
  date,
  location,
  description,
  debitFundAmounts,
  creditFundAmounts,
}: UseUpdateTransactionArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  updatedTransaction: Transaction | null;
  error: ApiError | null;
  updateTransaction: () => void;
} {
  const updateTransactionCallback = useCallback<
    () => Promise<Transaction | ApiError | null>
  >(async () => {
    const client = getApiClient();
    const { data, error } = await client.POST("/transactions/{transactionId}", {
      params: {
        path: {
          transactionId: existingTransaction.id,
        },
      },
      body: {
        date: date ? date.format("YYYY-MM-DD") : "",
        location,
        description,
        debitAccount:
          existingTransaction.debitAccount === null
            ? null
            : {
                fundAmounts: debitFundAmounts.map((fundAmount) => ({
                  fundId: fundAmount.fundId,
                  amount: fundAmount.amount,
                })),
              },
        creditAccount:
          existingTransaction.creditAccount === null
            ? null
            : {
                fundAmounts: creditFundAmounts.map((fundAmount) => ({
                  fundId: fundAmount.fundId,
                  amount: fundAmount.amount,
                })),
              },
      },
    });
    return typeof error === "undefined" ? data : error;
  }, [
    existingTransaction,
    date,
    location,
    description,
    debitFundAmounts,
    creditFundAmounts,
  ]);

  const {
    isRunning,
    isSuccess,
    response: updatedTransaction,
    error,
    execute,
  } = useApiRequest<Transaction>({
    apiRequestFunction: updateTransactionCallback,
  });

  return {
    isRunning,
    isSuccess,
    updatedTransaction,
    error,
    updateTransaction: execute,
  };
};

export default useUpdateTransaction;
