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
 * Interface representing the state of deleting a fund.
 */
type ActionState = WorkspaceActionState;

/**
 * Payload for the delete fund server action.
 */
interface ActionPayload extends WorkspaceActionPayload {
  readonly fundId: string;
}

/**
 * Server action that deletes an existing fund.
 */
const deleteFund = async function (
  _: ActionState,
  { fundId, redirectUrl }: ActionPayload,
): Promise<ActionState> {
  const client = await createApiClient();
  const { error } = await client.DELETE("/funds/{fundId}", {
    params: {
      path: {
        fundId,
      },
    },
  });
  if (error) {
    return mapActionError(error, {});
  }
  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default deleteFund;
