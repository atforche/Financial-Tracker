"use server";

import {
  type TransactionActionErrorState,
  mapTransactionActionError,
} from "@/transactions/workspace/transactionActionHelpers";
import createApiClient from "@/framework/data/createApiClient";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of unposting a transaction.
 */
interface ActionState extends TransactionActionErrorState {
  readonly success?: boolean;
}

/**
 * Payload for the unpost transaction server action.
 */
interface ActionPayload {
  readonly transactionId: string;
  readonly accountId?: string | null;
  readonly redirectUrl: string;
}

/**
 * Server action that unposts a transaction from the provided account, or from all accounts when none is provided.
 */
const unpostTransaction = async function (
  _: ActionState,
  { transactionId, accountId = null, redirectUrl }: ActionPayload,
): Promise<ActionState> {
  const client = await createApiClient();
  const { error } = await client.POST("/transactions/{transactionId}/unpost", {
    params: {
      path: {
        transactionId,
      },
      query: accountId === null ? {} : { accountId },
    },
  });
  if (error) {
    return mapTransactionActionError<never>(error, {});
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default unpostTransaction;
