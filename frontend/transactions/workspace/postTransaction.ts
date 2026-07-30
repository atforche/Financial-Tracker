"use server";

import {
  type TransactionActionErrorState,
  mapTransactionActionError,
} from "@/transactions/workspace/transactionActionHelpers";
import type { PostTransactionRequest } from "@/transactions/types";
import createApiClient from "@/framework/data/createApiClient";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of posting a transaction.
 */
interface ActionState extends TransactionActionErrorState {
  readonly success?: boolean;
  readonly accountErrors?: string | null;
  readonly dateErrors?: string | null;
}

/**
 * Payload for the post transaction server action.
 */
interface ActionPayload {
  readonly transactionId: string;
  readonly redirectUrl: string;
  readonly request: PostTransactionRequest;
}

/**
 * Server action that posts a transaction.
 */
const postTransaction = async function (
  _: ActionState,
  { transactionId, redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const client = createApiClient();
  const { error } = await client.POST("/transactions/{transactionId}/post", {
    params: {
      path: {
        transactionId,
      },
    },
    body: request,
  });
  if (error) {
    return mapTransactionActionError(error, {
      [propertyName<PostTransactionRequest>("accountId")]: "accountErrors",
      [propertyName<PostTransactionRequest>("date")]: "dateErrors",
    });
  }

  revalidatePath(redirectUrl);
  return { success: true };
};

export default postTransaction;
