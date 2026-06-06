"use server";

import type { PostTransactionRequest } from "@/transactions/types";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
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
  const client = getApiClient();
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
      let accountErrors = null;
      let dateErrors = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<PostTransactionRequest>("accountId").toUpperCase()
        ) {
          accountErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<PostTransactionRequest>("date").toUpperCase()
        ) {
          dateErrors = formatErrors(error.errors?.[key] ?? null);
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }
      return {
        errorTitle: error.title ?? null,
        accountErrors,
        dateErrors,
        unmappedErrors: unmappedErrors.filter(Boolean).join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  return { success: true };
};

export default postTransaction;
