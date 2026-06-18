"use server";

import type { UpdateTransactionRequest } from "@/transactions/types";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import { revalidatePath } from "next/cache";

interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly dateErrors?: string | null;
  readonly locationErrors?: string | null;
  readonly sourceLocationErrors?: string | null;
  readonly destinationLocationErrors?: string | null;
  readonly descriptionErrors?: string | null;
  readonly amountErrors?: string | null;
  readonly debitAccountErrors?: string | null;
  readonly creditAccountErrors?: string | null;
  readonly fundErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the update transaction server action.
 */
interface ActionPayload {
  readonly transactionId: string;
  readonly redirectUrl: string;
  readonly request: UpdateTransactionRequest;
}

/**
 * Server action that updates a transaction.
 */
const updateTransaction = async function (
  _: ActionState,
  { transactionId, redirectUrl, request }: ActionPayload,
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
      let sourceLocationErrors = null;
      let destinationLocationErrors = null;
      let descriptionErrors = null;
      let amountErrors = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        const normalizedKey = key.toUpperCase();

        if (normalizedKey === "DATE") {
          dateErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (normalizedKey === "LOCATION") {
          locationErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (normalizedKey === "SOURCELOCATION") {
          sourceLocationErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (normalizedKey === "DESTINATIONLOCATION") {
          destinationLocationErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (normalizedKey === "DESCRIPTION") {
          descriptionErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (normalizedKey === "AMOUNT") {
          amountErrors = formatErrors(error.errors?.[key] ?? null);
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }
      return {
        errorTitle: error.title ?? null,
        dateErrors,
        locationErrors,
        sourceLocationErrors,
        destinationLocationErrors,
        descriptionErrors,
        amountErrors,
        unmappedErrors: unmappedErrors.filter(Boolean).join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default updateTransaction;
