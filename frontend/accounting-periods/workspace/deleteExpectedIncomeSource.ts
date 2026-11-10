"use server";

import createApiClient from "@/framework/data/createApiClient";
import mapActionError from "@/framework/forms/mapActionError";
import { revalidatePath } from "next/cache";

/**
 * State of the delete expected income source server action.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the delete expected income source server action.
 */
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
    return mapActionError(error, {});
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default deleteExpectedIncomeSource;
