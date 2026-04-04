"use server";

import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of deleting a transaction.
 */
interface ActionState {
  readonly transactionId: string;
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that deletes an existing transaction.
 */
const deleteTransaction = async function ({
  transactionId,
  redirectUrl,
}: ActionState): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.DELETE("/transactions/{transactionId}", {
    params: {
      path: {
        transactionId,
      },
    },
  });
  if (error) {
    if (isApiError(error)) {
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        unmappedErrors.push(error.errors?.[key]?.join(" ") ?? null);
      }
      return {
        transactionId,
        redirectUrl,
        errorTitle: error.title ?? null,
        unmappedErrors: unmappedErrors.join(" ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default deleteTransaction;
