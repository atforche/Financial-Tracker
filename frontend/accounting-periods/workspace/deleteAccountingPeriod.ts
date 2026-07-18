"use server";

import {
  type AccountingPeriodActionPayload,
  type AccountingPeriodActionState,
  getAccountingPeriodActionError,
} from "@/accounting-periods/workspace/accountingPeriodAction";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import { revalidatePath } from "next/cache";

/**
 * Server action that deletes an existing accounting period.
 */
const deleteAccountingPeriod = async function (
  _: AccountingPeriodActionState,
  { accountingPeriodId, redirectUrl }: AccountingPeriodActionPayload,
): Promise<AccountingPeriodActionState> {
  const client = getApiClient();
  const { error } = await client.DELETE(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId,
        },
      },
    },
  );
  if (error) {
    if (isApiError(error)) {
      return getAccountingPeriodActionError(error);
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default deleteAccountingPeriod;
