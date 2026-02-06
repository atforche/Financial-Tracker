import { AccountType } from "@accounts/ApiTypes";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import type { ApiError } from "@data/ApiError";
import type { Dayjs } from "dayjs";
import type { FundAmount } from "@funds/ApiTypes";
import getApiClient from "@data/getApiClient";
import useApiRequest from "@data/useApiRequest";
import { useCallback } from "react";

/**
 * Interface representing the arguments for creating an Account.
 * @param name - Name for the Account.
 * @param type - Type for the Account.
 * @param accountingPeriod - Accounting Period for the Account.
 * @param addDate - Date to add the Account.
 * @param initialFundAmounts - Initial Fund amounts for the Account.
 */
interface UseCreateAccountArgs {
  readonly name: string;
  readonly type: AccountType | null;
  readonly accountingPeriod: AccountingPeriod | null;
  readonly addDate: Dayjs | null;
  readonly initialFundAmounts: FundAmount[];
}

/**
 * Hook used to create an Account via the API.
 * @param args - Arguments for creating an Account.
 * @returns Running state, success state, current error, and function to create the Account.
 */
const useCreateAccount = function ({
  name,
  type,
  accountingPeriod,
  addDate,
  initialFundAmounts,
}: UseCreateAccountArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  error: ApiError | null;
  createAccount: () => void;
} {
  const createAccountCallback = useCallback<
    () => Promise<ApiError | null>
  >(async () => {
    const client = getApiClient();
    const { error } = await client.POST("/accounts", {
      body: {
        name,
        type: type ?? AccountType.Standard,
        accountingPeriodId: accountingPeriod?.id ?? "",
        addDate: addDate ? addDate.format("YYYY-MM-DD") : "",
        initialFundAmounts: initialFundAmounts.map((fundAmount) => ({
          fundId: fundAmount.fundId,
          amount: fundAmount.amount,
        })),
      },
    });
    return typeof error === "undefined" ? null : error;
  }, [name, type, accountingPeriod, addDate, initialFundAmounts]);

  const { isRunning, isSuccess, error, execute } = useApiRequest({
    apiRequestFunction: createAccountCallback,
  });

  return { isRunning, isSuccess, error, createAccount: execute };
};

export default useCreateAccount;
