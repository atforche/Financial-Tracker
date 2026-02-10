import type { AccountIdentifier } from "@accounts/ApiTypes";
import type { ApiError } from "@data/ApiError";
import type { Dayjs } from "dayjs";
import type { Transaction } from "@transactions/ApiTypes";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for posting a Transaction.
 */
interface UsePostTransactionArgs {
  readonly transaction: Transaction;
  readonly account: AccountIdentifier | null;
  readonly date: Dayjs | null;
}

/**
 * Hook used to post a Transaction via the API.
 * @param args - Arguments for posting a Transaction.
 * @returns Running state, success state, current error, and function to post the Transaction.
 */
const usePostTransaction = function ({
  transaction,
  account,
  date,
}: UsePostTransactionArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  updatedTransaction: Transaction | null;
  error: ApiError | null;
  postTransaction: () => void;
} {
  const postTransactionCallback = useCallback<
    () => Promise<Transaction | ApiError | null>
  >(async () => {
    const client = getApiClient();
    const { data, error } = await client.POST(
      "/transactions/{transactionId}/post",
      {
        params: {
          path: {
            transactionId: transaction.id,
          },
        },
        body: {
          accountId: account?.id ?? "",
          date: date ? date.format("YYYY-MM-DD") : "",
        },
      },
    );
    return typeof error === "undefined" ? data : error;
  }, [transaction, account, date]);

  const {
    isRunning,
    isSuccess,
    response: updatedTransaction,
    error,
    execute,
  } = useApiRequest<Transaction>({
    apiRequestFunction: postTransactionCallback,
  });

  return {
    isRunning,
    isSuccess,
    updatedTransaction,
    error,
    postTransaction: execute,
  };
};

export default usePostTransaction;
