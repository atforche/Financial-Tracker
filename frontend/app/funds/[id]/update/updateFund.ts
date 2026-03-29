"use server";

import type { UpdateFundRequest } from "@/data/fundTypes";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of updating a fund.
 */
interface ActionState {
  readonly fundId: string;
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly nameErrors?: string | null;
  readonly descriptionErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that updates an existing fund.
 */
const updateFund = async function (
  { fundId, redirectUrl }: ActionState,
  request: UpdateFundRequest,
): Promise<ActionState> {
  const client = getApiClient();
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
      let nameErrorMessage = null;
      let descriptionErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() === nameof<UpdateFundRequest>("name").toUpperCase()
        ) {
          nameErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<UpdateFundRequest>("description").toUpperCase()
        ) {
          descriptionErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }
      return {
        fundId,
        redirectUrl,
        errorTitle: error.title ?? null,
        nameErrors: nameErrorMessage,
        descriptionErrors: descriptionErrorMessage,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default updateFund;
