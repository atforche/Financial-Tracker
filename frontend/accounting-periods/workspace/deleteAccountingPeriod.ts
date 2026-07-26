"use server";

import type {
  AccountingPeriodActionPayload,
  AccountingPeriodActionState,
} from "@/accounting-periods/workspace/accountingPeriodAction";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Server action that deletes an existing accounting period.
 */
const deleteAccountingPeriod = async function (
  _: AccountingPeriodActionState,
  { accountingPeriodId, redirectUrl }: AccountingPeriodActionPayload,
): Promise<AccountingPeriodActionState> {
  const client = createApiClient();
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
      return mapApiValidationError(error, {});
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default deleteAccountingPeriod;
