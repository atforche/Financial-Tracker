"use server";

import type { PostTransactionRequest } from "@/transactions/types";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of posting a transaction.
 */
interface ActionState {
  readonly transactionId: string;
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly accountErrors?: string | null;
  readonly dateErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that posts a transaction.
 */
const postTransaction = async function (
  { transactionId, redirectUrl }: ActionState,
  request: PostTransactionRequest,
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
        transactionId,
        redirectUrl,
        errorTitle: error.title ?? null,
        accountErrors,
        dateErrors,
        unmappedErrors: unmappedErrors.filter(Boolean).join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default postTransaction;
