"use server";

import type {
  WorkspaceActionPayload,
  WorkspaceActionState,
} from "@/framework/forms/workspaceAction";
import type { UpdateFundRequest } from "@/funds/types";
import createApiClient from "@/framework/data/createApiClient";
import mapActionError from "@/framework/forms/mapActionError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of updating a fund.
 */
interface ActionState extends WorkspaceActionState {
  readonly nameErrors?: string | null;
  readonly descriptionErrors?: string | null;
}

/**
 * Payload for the update server action.
 */
interface ActionPayload extends WorkspaceActionPayload {
  readonly fundId: string;
  readonly request: UpdateFundRequest;
}

/**
 * Server action that updates an existing fund.
 */
const updateFund = async function (
  _: ActionState,
  { fundId, redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const client = await createApiClient();
  const { error } = await client.POST("/funds/{fundId}", {
    params: {
      path: {
        fundId,
      },
    },
    body: request,
  });
  if (error) {
    return mapActionError(error, {
      [propertyName<UpdateFundRequest>("name")]: "nameErrors",
      [propertyName<UpdateFundRequest>("description")]: "descriptionErrors",
    });
  }

  revalidatePath(redirectUrl);
  return { success: true };
};

export default updateFund;
