"use server";

import type { UpdateTransactionRequest } from "@/transactions/types";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
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
  readonly locationErrors?: string | null;
  readonly descriptionErrors?: string | null;
  readonly amountErrors?: string | null;
  readonly debitAccountErrors?: string | null;
  readonly creditAccountErrors?: string | null;
  readonly fundErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that updates a transaction.
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
      let dateErrors = null;
      let locationErrors = null;
      let descriptionErrors = null;
      let amountErrors = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<UpdateTransactionRequest>("date").toUpperCase()
        ) {
          dateErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<UpdateTransactionRequest>("location").toUpperCase()
        ) {
          locationErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<UpdateTransactionRequest>("description").toUpperCase()
        ) {
          descriptionErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<UpdateTransactionRequest>("amount").toUpperCase()
        ) {
          amountErrors = formatErrors(error.errors?.[key] ?? null);
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }

      return {
        transactionId,
        redirectUrl,
        errorTitle: error.title ?? null,
        dateErrors,
        locationErrors,
        descriptionErrors,
        amountErrors,
        unmappedErrors: unmappedErrors.filter(Boolean).join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default updateTransaction;
