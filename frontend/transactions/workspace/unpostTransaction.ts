"use server";

import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of unposting a transaction.
 */
interface ActionState {
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
  const client = getApiClient();
  const { error } = await client.POST("/transactions/{transactionId}/unpost", {
    params: {
      path: {
        transactionId,
      },
    },
  });
  if (error) {
    if (isApiError(error)) {
      const unmappedErrors: (string | null)[] = [];

      for (const key of Object.keys(error.errors ?? {})) {
        unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
      }
      return {
        errorTitle: error.title ?? null,
        unmappedErrors: unmappedErrors.filter(Boolean).join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return {};
};

export default unpostTransaction;
