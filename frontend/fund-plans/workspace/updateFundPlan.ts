"use server";

import type { FundPlan, UpdateFundPlanRequest } from "@/fund-plans/types";
import createApiClient from "@/framework/data/createApiClient";
import { revalidatePath } from "next/cache";

/**
 * State of the updateFundPlan action.
 */
interface ActionState {
  readonly success?: boolean;
}

/**
 * Payload for the updateFundPlan action.
 */
interface ActionPayload {
  readonly fundPlan: FundPlan;
  readonly request: UpdateFundPlanRequest;
  readonly redirectUrl: string;
}

/**
 * Updates a Fund Plan with the provided request data and revalidates the specified path.
 */
const updateFundPlan = async function (
  _: ActionState,
  payload: ActionPayload,
): Promise<ActionState> {
  await createApiClient().POST("/fund-plans/{fundPlanId}", {
    params: { path: { fundPlanId: payload.fundPlan.id } },
    body: payload.request,
  });
  revalidatePath(payload.redirectUrl);
  return { success: true };
};

export default updateFundPlan;
