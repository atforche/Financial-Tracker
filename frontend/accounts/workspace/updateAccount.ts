"use server";

import type {
  AccountActionPayload,
  AccountActionState,
} from "@/accounts/workspace/accountAction";
import type { UpdateAccountRequest } from "@/accounts/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
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
  const apiClient = createApiClient();
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
      const formattedError = mapApiValidationError(error, {
        [propertyName<UpdateAccountRequest>("name")]: "nameErrors",
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
