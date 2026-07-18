"use server";

import type { OnboardAccountRequest } from "@/accounts/types";
import formatAccountActionError from "@/accounts/workspace/formatAccountActionError";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of onboarding an account.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly nameErrors?: string | null;
  readonly typeErrors?: string | null;
  readonly onboardedBalanceErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the onboarding server action.
 */
interface ActionPayload {
  readonly redirectUrl: string;
  readonly request: OnboardAccountRequest;
}

/**
 * Server action that onboards a new account before any accounting periods exist.
 */
const onboardAccount = async function (
  _: ActionState,
  { redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const apiClient = getApiClient();
  const { error } = await apiClient.POST("/accounts/onboard", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      const formattedError = formatAccountActionError(error, {
        [nameof<OnboardAccountRequest>("name")]: "nameErrors",
        [nameof<OnboardAccountRequest>("type")]: "typeErrors",
        [nameof<OnboardAccountRequest>("onboardedBalance")]:
          "onboardedBalanceErrors",
      });
      return {
        errorTitle: formattedError.errorTitle,
        nameErrors: formattedError.fieldErrors["nameErrors"] ?? null,
        typeErrors: formattedError.fieldErrors["typeErrors"] ?? null,
        onboardedBalanceErrors:
          formattedError.fieldErrors["onboardedBalanceErrors"] ?? null,
        unmappedErrors: formattedError.unmappedErrors,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default onboardAccount;
