"use server";

import type { UpdateTransactionRequest } from "@/transactions/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
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

/** Validation fields returned for nested transaction request models. */
interface TransactionValidationFields {
  readonly location: string;
  readonly sourceLocation: string;
  readonly destinationLocation: string;
}

/**
 * Server action that updates a transaction.
 */
const updateTransaction = async function (
  _: ActionState,
  { transactionId, redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const client = createApiClient();
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
      const mappedError = mapApiValidationError(error, {
        [propertyName<UpdateTransactionRequest>("date")]: "dateErrors",
        [propertyName<TransactionValidationFields>("location")]:
          "locationErrors",
        [propertyName<TransactionValidationFields>("sourceLocation")]:
          "sourceLocationErrors",
        [propertyName<TransactionValidationFields>("destinationLocation")]:
          "destinationLocationErrors",
        [propertyName<UpdateTransactionRequest>("description")]:
          "descriptionErrors",
        [propertyName<UpdateTransactionRequest>("amount")]: "amountErrors",
      });
      return {
        ...mappedError,
        ...mappedError.fieldErrors,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default updateTransaction;
