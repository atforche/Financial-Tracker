"use server";

import type {
  FundActionPayload,
  FundActionState,
} from "@/funds/workspace/fundAction";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of deleting a fund.
 */
type ActionState = FundActionState;

/**
 * Payload for the delete fund server action.
 */
interface ActionPayload extends FundActionPayload {
  readonly fundId: string;
}

/**
 * Server action that deletes an existing fund.
 */
const deleteFund = async function (
  _: ActionState,
  { fundId, redirectUrl }: ActionPayload,
): Promise<ActionState> {
  const client = createApiClient();
  const { error } = await client.DELETE("/funds/{fundId}", {
    params: {
      path: {
        fundId,
      },
    },
  });
  if (error) {
    if (isApiError(error)) {
      return mapApiValidationError(error, {});
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default deleteFund;
