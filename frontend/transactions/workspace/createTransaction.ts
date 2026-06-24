"use server";

import type { CreateTransactionRequest } from "@/transactions/transaction";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import { revalidatePath } from "next/cache";

interface ActionState {
  readonly success?: boolean;
  readonly transactionId?: string | null;
  readonly errorTitle?: string | null;
  readonly accountingPeriodErrors?: string | null;
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
 * Payload for the create transaction server action.
 */
interface ActionPayload {
  readonly redirectUrl: string;
  readonly request: CreateTransactionRequest;
}

/**
 * Server action that creates a transaction.
 */
const createTransaction = async function (
  _: ActionState,
  { redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const client = getApiClient();
  const { data, error } = await client.POST("/transactions", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      let accountingPeriodErrors = null;
      let dateErrors = null;
      let locationErrors = null;
      let sourceLocationErrors = null;
      let destinationLocationErrors = null;
      let descriptionErrors = null;
      let amountErrors = null;
      const unmappedErrors: (string | null)[] = [];

      for (const key of Object.keys(error.errors ?? {})) {
        const normalizedKey = key.toUpperCase();

        if (normalizedKey === "ACCOUNTINGPERIODID") {
          accountingPeriodErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (normalizedKey === "DATE") {
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
        accountingPeriodErrors,
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
  if (typeof data === "undefined") {
    throw new Error("Transaction creation did not return a transaction.");
  }
  revalidatePath(redirectUrl);
  return { success: true, transactionId: data.id };
};

export default createTransaction;
