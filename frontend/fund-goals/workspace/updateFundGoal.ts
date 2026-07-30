"use server";

import type { FundGoal, UpdateFundGoalRequest } from "@/fund-goals/types";
import createApiClient from "@/framework/data/createApiClient";
import { revalidatePath } from "next/cache";

/**
 * State of the updateFundGoal action.
 */
interface ActionState {
  readonly success?: boolean;
}

/**
 * Payload for the updateFundGoal action.
 */
interface ActionPayload {
  readonly fundGoal: FundGoal;
  readonly request: UpdateFundGoalRequest;
  readonly redirectUrl: string;
}

/**
 * Updates a Fund Goal with the provided request data and revalidates the specified path.
 */
const updateFundGoal = async function (
  _: ActionState,
  payload: ActionPayload,
): Promise<ActionState> {
  await createApiClient().POST("/fund-goals/{fundGoalId}", {
    params: { path: { fundGoalId: payload.fundGoal.id } },
    body: payload.request,
  });
  revalidatePath(payload.redirectUrl);
  return { success: true };
};

export default updateFundGoal;
