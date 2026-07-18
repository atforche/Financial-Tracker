"use server";

import formatAccountActionError from "@/accounts/workspace/formatAccountActionError";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of deleting an account.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the delete account server action.
 */
interface ActionPayload {
  readonly accountId: string;
  readonly redirectUrl: string;
}

/**
 * Server action that deletes an existing account.
 */
const deleteAccount = async function (
  _: ActionState,
  { accountId, redirectUrl }: ActionPayload,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.DELETE("/accounts/{accountId}", {
    params: {
      path: {
        accountId,
      },
    },
  });
  if (error) {
    if (isApiError(error)) {
      const formattedError = formatAccountActionError(error, {});
      return {
        errorTitle: formattedError.errorTitle,
        unmappedErrors: formattedError.unmappedErrors,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default deleteAccount;
