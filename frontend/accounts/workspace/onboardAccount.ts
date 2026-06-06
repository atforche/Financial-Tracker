"use server";

import type { OnboardAccountRequest } from "@/accounts/types";
import formatErrors from "@/framework/forms/formatErrors";
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
      let nameErrorMessage = null;
      let typeErrorMessage = null;
      let onboardedBalanceErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];

      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<OnboardAccountRequest>("name").toUpperCase()
        ) {
          nameErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<OnboardAccountRequest>("type").toUpperCase()
        ) {
          typeErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<OnboardAccountRequest>("onboardedBalance").toUpperCase()
        ) {
          onboardedBalanceErrorMessage = formatErrors(
            error.errors?.[key] ?? null,
          );
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }
      return {
        success: false,
        errorTitle: error.title ?? null,
        nameErrors: nameErrorMessage,
        typeErrors: typeErrorMessage,
        onboardedBalanceErrors: onboardedBalanceErrorMessage,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default onboardAccount;
