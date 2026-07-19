"use server";

import type { CreateTransactionRequest } from "@/transactions/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating a transaction.
 */
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

/** Validation fields returned for nested transaction request models. */
interface TransactionValidationFields {
  readonly location: string;
  readonly sourceLocation: string;
  readonly destinationLocation: string;
}

/**
 * Server action that creates a transaction.
 */
const createTransaction = async function (
  _: ActionState,
  { redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const client = createApiClient();
  const { data, error } = await client.POST("/transactions", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      const mappedError = mapApiValidationError(error, {
        [propertyName<CreateTransactionRequest>("accountingPeriodId")]:
          "accountingPeriodErrors",
        [propertyName<CreateTransactionRequest>("date")]: "dateErrors",
        [propertyName<TransactionValidationFields>("location")]:
          "locationErrors",
        [propertyName<TransactionValidationFields>("sourceLocation")]:
          "sourceLocationErrors",
        [propertyName<TransactionValidationFields>("destinationLocation")]:
          "destinationLocationErrors",
        [propertyName<CreateTransactionRequest>("description")]:
          "descriptionErrors",
        [propertyName<CreateTransactionRequest>("amount")]: "amountErrors",
      });
      return {
        ...mappedError,
        ...mappedError.fieldErrors,
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
