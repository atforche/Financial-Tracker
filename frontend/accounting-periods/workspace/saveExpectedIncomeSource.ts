"use server";

import type { ExpectedIncomeSourceRequest } from "@/accounting-periods/types";
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
  readonly expectedIncomeSourceId?: string;
  readonly redirectUrl: string;
  readonly source: ExpectedIncomeSourceRequest;
}

/**
 * Adds or updates one expected-income source for an open Accounting Period.
 */
const saveExpectedIncomeSource = async function (
  _: ActionState,
  {
    accountingPeriodId,
    expectedIncomeSourceId,
    redirectUrl,
    source,
  }: ActionPayload,
): Promise<ActionState> {
  const client = await createApiClient();
  const { error } =
    expectedIncomeSourceId === undefined
      ? await client.POST(
          "/accounting-periods/{accountingPeriodId}/expected-income-sources",
          { params: { path: { accountingPeriodId } }, body: source },
        )
      : await client.POST(
          "/accounting-periods/{accountingPeriodId}/expected-income-sources/{expectedIncomeSourceId}",
          {
            params: {
              path: { accountingPeriodId, expectedIncomeSourceId },
            },
            body: source,
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

export default saveExpectedIncomeSource;
