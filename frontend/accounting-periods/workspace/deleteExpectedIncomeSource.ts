"use server";

import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import { revalidatePath } from "next/cache";

interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

interface ActionPayload {
  readonly accountingPeriodId: string;
  readonly expectedIncomeSourceId: string;
  readonly redirectUrl: string;
}

/**
 * Deletes one expected-income source from an open Accounting Period.
 */
const deleteExpectedIncomeSource = async function (
  _: ActionState,
  { accountingPeriodId, expectedIncomeSourceId, redirectUrl }: ActionPayload,
): Promise<ActionState> {
  const client = await createApiClient();
  const { error } = await client.DELETE(
    "/accounting-periods/{accountingPeriodId}/expected-income-sources/{expectedIncomeSourceId}",
    {
      params: { path: { accountingPeriodId, expectedIncomeSourceId } },
    },
  );
  if (error) {
    if (isApiError(error)) {
      return mapApiValidationError(error, {});
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default deleteExpectedIncomeSource;
