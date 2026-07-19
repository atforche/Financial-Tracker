"use server";

import type {
  AccountActionPayload,
  AccountActionState,
} from "@/accounts/workspace/accountAction";
import formatAccountActionError from "@/accounts/workspace/formatAccountActionError";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of deleting an account.
 */
type ActionState = AccountActionState;

/**
 * Payload for the delete account server action.
 */
interface ActionPayload extends AccountActionPayload {
  readonly accountId: string;
}

/**
 * Server action that deletes an existing account.
 */
const deleteAccount = async function (
  _: ActionState,
  { accountId, redirectUrl }: ActionPayload,
): Promise<ActionState> {
  const apiClient = getApiClient();
  const { error } = await apiClient.DELETE("/accounts/{accountId}", {
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
