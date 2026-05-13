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
 * Server action that onboards a new account before any accounting periods exist.
 */
const onboardAccount = async function (
  _: ActionState,
  request: OnboardAccountRequest,
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
        errorTitle: error.title ?? null,
        nameErrors: nameErrorMessage,
        typeErrors: typeErrorMessage,
        onboardedBalanceErrors: onboardedBalanceErrorMessage,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(routes.index({}));
  redirect(routes.index({}));
};

export default onboardAccount;
