"use server";

import {
  type TransactionActionErrorState,
  mapTransactionActionError,
} from "@/transactions/workspace/transactionActionHelpers";
import type { UpdateTransactionRequest } from "@/transactions/types";
import createApiClient from "@/framework/data/createApiClient";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of updating a transaction, including success status and any validation errors.
 */
interface ActionState extends TransactionActionErrorState {
  readonly success?: boolean;
  readonly dateErrors?: string | null;
  readonly locationErrors?: string | null;
  readonly sourceLocationErrors?: string | null;
  readonly destinationLocationErrors?: string | null;
  readonly descriptionErrors?: string | null;
  readonly amountErrors?: string | null;
  readonly debitAccountErrors?: string | null;
  readonly creditAccountErrors?: string | null;
  readonly fundErrors?: string | null;
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
    return mapTransactionActionError(error, {
      [propertyName<UpdateTransactionRequest>("date")]: "dateErrors",
      [propertyName<TransactionValidationFields>("location")]: "locationErrors",
      [propertyName<TransactionValidationFields>("sourceLocation")]:
        "sourceLocationErrors",
      [propertyName<TransactionValidationFields>("destinationLocation")]:
        "destinationLocationErrors",
      [propertyName<UpdateTransactionRequest>("description")]:
        "descriptionErrors",
      [propertyName<UpdateTransactionRequest>("amount")]: "amountErrors",
    });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default updateTransaction;
