"use server";

import type {
  AccountActionPayload,
  AccountActionState,
} from "@/accounts/workspace/accountAction";
import type { UpdateAccountRequest } from "@/accounts/types";
import formatAccountActionError from "@/accounts/workspace/formatAccountActionError";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of updating an account.
 */
interface ActionState extends AccountActionState {
  readonly nameErrors?: string | null;
}

/**
 * Payload for the update server action.
 */
interface ActionPayload extends AccountActionPayload {
  readonly accountId: string;
  readonly request: UpdateAccountRequest;
}

/**
 * Server action that updates an existing account.
 */
const updateAccount = async function (
  _: ActionState,
  { accountId, redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const apiClient = getApiClient();
  const { error } = await apiClient.POST("/accounts/{accountId}", {
    params: {
      path: {
        accountId,
      },
    },
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      const formattedError = formatAccountActionError(error, {
        [nameof<UpdateAccountRequest>("name")]: "nameErrors",
      });
      return {
        errorTitle: formattedError.errorTitle,
        nameErrors: formattedError.fieldErrors.nameErrors ?? null,
        unmappedErrors: formattedError.unmappedErrors,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  return { success: true };
};

export default updateAccount;
