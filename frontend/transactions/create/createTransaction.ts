"use server";

import type { CreateTransactionRequest } from "@/transactions/types";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating a transaction.
 */
interface ActionState {
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly accountingPeriodErrors?: string | null;
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
 * Server action that creates a transaction.
 */
const createTransaction = async function (
  { redirectUrl }: ActionState,
  request: CreateTransactionRequest,
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
      let descriptionErrors = null;
      let amountErrors = null;
      const unmappedErrors: (string | null)[] = [];

      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<CreateTransactionRequest>("accountingPeriodId").toUpperCase()
        ) {
          accountingPeriodErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateTransactionRequest>("date").toUpperCase()
        ) {
          dateErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateTransactionRequest>("location").toUpperCase()
        ) {
          locationErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateTransactionRequest>("description").toUpperCase()
        ) {
          descriptionErrors = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateTransactionRequest>("amount").toUpperCase()
        ) {
          amountErrors = formatErrors(error.errors?.[key] ?? null);
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }

      return {
        redirectUrl,
        errorTitle: error.title ?? null,
        accountingPeriodErrors,
        dateErrors,
        locationErrors,
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
  redirect(redirectUrl);
};

export default createTransaction;
