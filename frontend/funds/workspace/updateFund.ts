"use server";

import type { UpdateFundRequest } from "@/funds/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of updating a fund.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly nameErrors?: string | null;
  readonly descriptionErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the update server action.
 */
interface ActionPayload {
  readonly fundId: string;
  readonly redirectUrl: string;
  readonly request: UpdateFundRequest;
}

/**
 * Server action that updates an existing fund.
 */
const updateFund = async function (
  _: ActionState,
  { fundId, redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const client = createApiClient();
  const { error } = await client.POST("/funds/{fundId}", {
    params: {
      path: {
        fundId,
      },
    },
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      const mappedError = mapApiValidationError(error, {
        [propertyName<UpdateFundRequest>("name")]: "nameErrors",
        [propertyName<UpdateFundRequest>("description")]: "descriptionErrors",
      });
      return {
        ...mappedError,
        ...mappedError.fieldErrors,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  return { success: true };
};

export default updateFund;
