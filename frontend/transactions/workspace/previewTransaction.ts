"use server";

import type {
  CreateTransactionRequest,
  Transaction,
  UpdateTransactionRequest,
} from "@/transactions/types";
import createApiClient from "@/framework/data/createApiClient";

/**
 * Payload for the preview transaction server action: a create request, or an update request for an
 * existing transaction.
 */
type TransactionPreviewPayload =
  | {
      readonly transactionId: null;
      readonly request: CreateTransactionRequest;
    }
  | {
      readonly transactionId: string;
      readonly request: UpdateTransactionRequest;
    };

/**
 * Server action that previews the transaction a create or update request would produce, including the
 * balances it would have once saved. Returns null when the request cannot be previewed.
 */
const previewTransaction = async function (
  payload: TransactionPreviewPayload,
): Promise<Transaction | null> {
  const client = await createApiClient();
  const { data } =
    payload.transactionId === null
      ? await client.POST("/transactions/preview", { body: payload.request })
      : await client.POST("/transactions/{transactionId}/preview", {
          params: { path: { transactionId: payload.transactionId } },
          body: payload.request,
        });
  return data ?? null;
};

export type { TransactionPreviewPayload };
export default previewTransaction;
