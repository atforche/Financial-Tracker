"use server";

import type {
  AccountActionPayload,
  AccountActionState,
} from "@/accounts/workspace/accountAction";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
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
  const apiClient = createApiClient();
  const { error } = await apiClient.DELETE("/accounts/{accountId}", {
    params: {
      path: {
        accountId,
      },
    },
  });
  if (error) {
    if (isApiError(error)) {
      const formattedError = mapApiValidationError(error, {});
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
