"use server";

import type {
  WorkspaceActionPayload,
  WorkspaceActionState,
} from "@/framework/forms/workspaceAction";
import type { UpdateAccountRequest } from "@/accounts/types";
import createApiClient from "@/framework/data/createApiClient";
import mapActionError from "@/framework/forms/mapActionError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of updating an account.
 */
interface ActionState extends WorkspaceActionState {
  readonly nameErrors?: string | null;
}

/**
 * Payload for the update server action.
 */
interface ActionPayload extends WorkspaceActionPayload {
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
  const apiClient = await createApiClient();
  const { error } = await apiClient.POST("/accounts/{accountId}", {
    params: {
      path: {
        accountId,
      },
    },
    body: request,
  });
  if (error) {
    return mapActionError(error, {
      [propertyName<UpdateAccountRequest>("name")]: "nameErrors",
    });
  }

  revalidatePath(redirectUrl);
  return { success: true };
};

export default updateAccount;
