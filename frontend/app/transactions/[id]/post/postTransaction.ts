"use server";

import type { PostTransactionRequest } from "@/data/transactionTypes";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of posting a transaction to an account.
 */
interface ActionState {
  readonly transactionId: string;
  readonly accountId: string;
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly dateErrors?: string | null;
  readonly accountErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that posts an existing transaction to an account.
 */
const postTransaction = async function (
  { transactionId, accountId, redirectUrl }: ActionState,
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
      let dateErrorMessage = null;
      let accountErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<PostTransactionRequest>("date").toUpperCase()
        ) {
          dateErrorMessage = error.errors?.[key]?.join(" ") ?? null;
        } else if (
          key.toUpperCase() ===
          nameof<PostTransactionRequest>("accountId").toUpperCase()
        ) {
          accountErrorMessage = error.errors?.[key]?.join(" ") ?? null;
        } else {
          unmappedErrors.push(error.errors?.[key]?.join(" ") ?? null);
        }
      }
      return {
        transactionId,
        accountId,
        redirectUrl,
        errorTitle: error.title ?? null,
        dateErrors: dateErrorMessage,
        accountErrors: accountErrorMessage,
        unmappedErrors: unmappedErrors.join(" ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default postTransaction;
