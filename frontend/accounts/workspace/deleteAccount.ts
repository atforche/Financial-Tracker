"use server";

import type {
  WorkspaceActionPayload,
  WorkspaceActionState,
} from "@/framework/forms/workspaceAction";
import createApiClient from "@/framework/data/createApiClient";
import mapActionError from "@/framework/forms/mapActionError";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of deleting an account.
 */
type ActionState = WorkspaceActionState;

/**
 * Payload for the delete account server action.
 */
interface ActionPayload extends WorkspaceActionPayload {
  readonly accountId: string;
}

/**
 * Server action that deletes an existing account.
 */
const deleteAccount = async function (
  _: ActionState,
  { accountId, redirectUrl }: ActionPayload,
): Promise<ActionState> {
  const apiClient = await createApiClient();
  const { error } = await apiClient.DELETE("/accounts/{accountId}", {
    params: {
      path: {
        accountId,
      },
    },
  });
  if (error) {
    return mapActionError(error, {});
  }
  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default deleteAccount;
