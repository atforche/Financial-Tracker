import type { Account } from "@accounts/ApiTypes";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import type { ApiError } from "@data/ApiError";
import type { Dayjs } from "dayjs";
import type { FundAmount } from "@funds/ApiTypes";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for creating a Transaction.
 */
interface UseCreateTransactionArgs {
  readonly accountingPeriod: AccountingPeriod | null;
  readonly date: Dayjs | null;
  readonly location: string;
  readonly description: string;
  readonly debitAccount: Account | null;
  readonly debitFundAmounts: FundAmount[];
  readonly creditAccount: Account | null;
  readonly creditFundAmounts: FundAmount[];
}

/**
 * Hook used to create a Transaction via the API.
 * @param args - Arguments for creating a Transaction.
 * @returns Running state, success state, current error, and function to create the Transaction.
 */
const useCreateTransaction = function ({
  accountingPeriod,
  date,
  location,
  description,
  debitAccount,
  debitFundAmounts,
  creditAccount,
  creditFundAmounts,
}: UseCreateTransactionArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  error: ApiError | null;
  createTransaction: () => void;
} {
  const createTransactionCallback = useCallback<
    () => Promise<ApiError | null>
  >(async () => {
    const client = getApiClient();
    const { error } = await client.POST("/transactions", {
      body: {
        accountingPeriodId: accountingPeriod?.id ?? "",
        date: date ? date.format("YYYY-MM-DD") : "",
        location,
        description,
        debitAccount:
          debitAccount === null
            ? null
            : {
                accountId: debitAccount.id,
                fundAmounts: debitFundAmounts.map((fundAmount) => ({
                  fundId: fundAmount.fundId,
                  amount: fundAmount.amount,
                })),
              },
        creditAccount:
          creditAccount === null
            ? null
            : {
                accountId: creditAccount.id,
                fundAmounts: creditFundAmounts.map((fundAmount) => ({
                  fundId: fundAmount.fundId,
                  amount: fundAmount.amount,
                })),
              },
      },
    });
    return typeof error === "undefined" ? null : error;
  }, [
    accountingPeriod,
    date,
    location,
    description,
    debitAccount,
    debitFundAmounts,
    creditAccount,
    creditFundAmounts,
  ]);

  const { isRunning, isSuccess, error, execute } = useApiRequest({
    apiRequestFunction: createTransactionCallback,
  });

  return { isRunning, isSuccess, error, createTransaction: execute };
};

export default useCreateTransaction;
