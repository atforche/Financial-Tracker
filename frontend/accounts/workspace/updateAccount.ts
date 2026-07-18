"use server";

import type { UpdateAccountRequest } from "@/accounts/types";
import formatAccountActionError from "@/accounts/workspace/formatAccountActionError";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of updating an account.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly nameErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the update server action.
 */
interface ActionPayload {
  readonly accountId: string;
  readonly redirectUrl: string;
  readonly request: UpdateAccountRequest;
}

/**
 * Server action that updates an existing account.
 */
const updateAccount = async function (
  _: ActionState,
  { accountId, redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST("/accounts/{accountId}", {
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
        nameErrors: formattedError.fieldErrors["nameErrors"] ?? null,
        unmappedErrors: formattedError.unmappedErrors,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  return { success: true };
};

export default updateAccount;
