"use server";

import {
  type TransactionActionErrorState,
  mapTransactionActionError,
} from "@/transactions/workspace/transactionActionHelpers";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of deleting a transaction.
 */
interface ActionState extends TransactionActionErrorState {
  readonly success?: boolean;
}

/**
 * Payload for the delete transaction server action.
 */
interface ActionPayload {
  readonly transactionId: string;
  readonly redirectUrl: string;
}

/**
 * Server action that deletes a transaction.
 */
const deleteTransaction = async function (
  _: ActionState,
  { transactionId, redirectUrl }: ActionPayload,
): Promise<ActionState> {
  const client = createApiClient();
  const { error } = await client.DELETE("/transactions/{transactionId}", {
    params: {
      path: {
        transactionId,
      },
    },
  });
  if (error) {
    return mapTransactionActionError<never>(error, {});
  }
  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default deleteTransaction;
