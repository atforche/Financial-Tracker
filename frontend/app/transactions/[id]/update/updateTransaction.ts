"use server";

import type { UpdateTransactionRequest } from "@/data/transactionTypes";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of updating a transaction.
 */
interface ActionState {
  readonly transactionId: string;
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly dateErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that updates an existing transaction.
 */
const updateTransaction = async function (
  { transactionId, redirectUrl }: ActionState,
  request: UpdateTransactionRequest,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST("/transactions/{transactionId}", {
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
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<UpdateTransactionRequest>("date").toUpperCase()
        ) {
          dateErrorMessage = error.errors?.[key]?.join(" ") ?? null;
        } else {
          unmappedErrors.push(error.errors?.[key]?.join(" ") ?? null);
        }
      }
      return {
        transactionId,
        redirectUrl,
        errorTitle: error.title ?? null,
        dateErrors: dateErrorMessage,
        unmappedErrors: unmappedErrors.join(" ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default updateTransaction;
