"use server";

import type { ExpectedIncomeSourceRequest } from "@/accounting-periods/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import { revalidatePath } from "next/cache";

/**
 * State returned by the updateExpectedIncomeSources action.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the updateExpectedIncomeSources action.
 */
interface ActionPayload {
  readonly accountingPeriodId: string;
  readonly redirectUrl: string;
  readonly sources: ExpectedIncomeSourceRequest[];
}

/**
 * Updates expected-income sources for an open Accounting Period.
 */
const updateExpectedIncomeSources = async function (
  _: ActionState,
  { accountingPeriodId, redirectUrl, sources }: ActionPayload,
): Promise<ActionState> {
  const client = await createApiClient();
  const { error } = await client.POST(
    "/accounting-periods/{accountingPeriodId}/expected-income-sources",
    { params: { path: { accountingPeriodId } }, body: sources },
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

export default updateExpectedIncomeSources;
