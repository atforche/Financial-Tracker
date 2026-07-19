"use server";

import type { PostTransactionRequest } from "@/transactions/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of posting a transaction.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly accountErrors?: string | null;
  readonly dateErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the post transaction server action.
 */
interface ActionPayload {
  readonly transactionId: string;
  readonly redirectUrl: string;
  readonly request: PostTransactionRequest;
}

/**
 * Server action that posts a transaction.
 */
const postTransaction = async function (
  _: ActionState,
  { transactionId, redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const client = createApiClient();
  const { error } = await client.POST("/transactions/{transactionId}/post", {
    params: {
      path: {
        transactionId,
      },
    },
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      const mappedError = mapApiValidationError(error, {
        [propertyName<PostTransactionRequest>("accountId")]: "accountErrors",
        [propertyName<PostTransactionRequest>("date")]: "dateErrors",
      });
      return {
        ...mappedError,
        ...mappedError.fieldErrors,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  return { success: true };
};

export default postTransaction;
