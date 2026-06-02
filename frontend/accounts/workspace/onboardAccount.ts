"use server";

import type { OnboardAccountRequest } from "@/accounts/types";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";
import routes from "@/accounts/routes";

/**
 * Interface representing the state of onboarding an account.
 */
interface ActionState {
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
 * Ensures redirects stay within the app.
 */
const getSafeRedirectUrl = function (redirectUrl: string): string {
  if (redirectUrl.startsWith("/") && !redirectUrl.startsWith("//")) {
    return redirectUrl;
  }
  return routes.workspace({});
};

/**
 * Server action that onboards a new account before any accounting periods exist.
 */
const onboardAccount = async function (
  _: ActionState,
  payload: OnboardAccountRequest | ActionPayload,
): Promise<ActionState> {
  const request = "request" in payload ? payload.request : payload;
  const redirectUrl =
    "request" in payload
      ? getSafeRedirectUrl(payload.redirectUrl)
      : routes.workspace({});
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
  redirect(redirectUrl);
};

export default onboardAccount;
