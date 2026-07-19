"use server";

import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of unposting a transaction.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the unpost transaction server action.
 */
interface ActionPayload {
  readonly transactionId: string;
  readonly redirectUrl: string;
}

/**
 * Server action that unposts a transaction.
 */
const unpostTransaction = async function (
  _: ActionState,
  { transactionId, redirectUrl }: ActionPayload,
): Promise<ActionState> {
  const client = createApiClient();
  const { error } = await client.POST("/transactions/{transactionId}/unpost", {
    params: {
      path: {
        transactionId,
      },
    },
  });
  if (error) {
    if (isApiError(error)) {
      return mapApiValidationError(error, {});
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default unpostTransaction;
