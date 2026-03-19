"use server";

import type { CreateTransactionRequest } from "@/data/transactionTypes";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating a transaction.
 */
interface ActionState {
  readonly errorTitle?: string | null;
  readonly dateErrors?: string | null;
  readonly debitAccountErrors?: string | null;
  readonly creditAccountErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that creates a new transaction.
 */
const createTransaction = async function (
  _: ActionState,
  request: CreateTransactionRequest,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST("/transactions", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      let dateErrorMessage = null;
      let debitAccountErrorMessage = null;
      let creditAccountErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<CreateTransactionRequest>("date").toUpperCase()
        ) {
          dateErrorMessage = error.errors?.[key]?.join(" ") ?? null;
        } else if (
          key.toUpperCase() ===
          nameof<CreateTransactionRequest>("debitAccount").toUpperCase()
        ) {
          debitAccountErrorMessage = error.errors?.[key]?.join(" ") ?? null;
        } else if (
          key.toUpperCase() ===
          nameof<CreateTransactionRequest>("creditAccount").toUpperCase()
        ) {
          creditAccountErrorMessage = error.errors?.[key]?.join(" ") ?? null;
        } else {
          unmappedErrors.push(error.errors?.[key]?.join(" ") ?? null);
        }
      }
      return {
        errorTitle: error.title ?? null,
        dateErrors: dateErrorMessage,
        debitAccountErrors: debitAccountErrorMessage,
        creditAccountErrors: creditAccountErrorMessage,
        unmappedErrors: unmappedErrors.join(" ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(`/accounting-periods/${request.accountingPeriodId}`);
  redirect(`/accounting-periods/${request.accountingPeriodId}`);
};

export default createTransaction;
