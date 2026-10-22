"use server";

import type { Transaction } from "@/transactions/types";
import createApiClient from "@/framework/data/createApiClient";

/**
 * Payload for the preview transaction posting server action.
 */
interface TransactionPostingPreviewPayload {
  readonly transactionId: string;
  readonly accountId: string;
  readonly date: string;
}

/**
 * Server action that previews a transaction as it would be immediately after posting it to the provided
 * account on the provided date. Returns null when the posting cannot be previewed.
 */
const previewTransactionPosting = async function ({
  transactionId,
  accountId,
  date,
}: TransactionPostingPreviewPayload): Promise<Transaction | null> {
  const client = await createApiClient();
  const { data } = await client.POST(
    "/transactions/{transactionId}/post/preview",
    {
      params: { path: { transactionId } },
      body: { accountId, date },
    },
  );
  return data ?? null;
};

export type { TransactionPostingPreviewPayload };
export default previewTransactionPosting;
