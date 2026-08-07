"use server";

import type {
  AccountActionPayload,
  AccountActionState,
} from "@/accounts/workspace/accountAction";
import type { OnboardAccountRequest } from "@/accounts/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of onboarding an account.
 */
interface ActionState extends AccountActionState {
  readonly nameErrors?: string | null;
  readonly typeErrors?: string | null;
  readonly onboardedBalanceErrors?: string | null;
}

/**
 * Payload for the onboarding server action.
 */
interface ActionPayload extends AccountActionPayload {
  readonly request: OnboardAccountRequest;
}

/**
 * Server action that onboards a new account before any accounting periods exist.
 */
const onboardAccount = async function (
  _: ActionState,
  { redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const apiClient = await createApiClient();
  const { error } = await apiClient.POST("/accounts/onboard", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      const formattedError = mapApiValidationError(error, {
        [propertyName<OnboardAccountRequest>("name")]: "nameErrors",
        [propertyName<OnboardAccountRequest>("type")]: "typeErrors",
        [propertyName<OnboardAccountRequest>("onboardedBalance")]:
          "onboardedBalanceErrors",
      });
      return {
        errorTitle: formattedError.errorTitle,
        nameErrors: formattedError.fieldErrors.nameErrors ?? null,
        typeErrors: formattedError.fieldErrors.typeErrors ?? null,
        onboardedBalanceErrors:
          formattedError.fieldErrors.onboardedBalanceErrors ?? null,
        unmappedErrors: formattedError.unmappedErrors,
      };
    }
    return {
      errorTitle: "Request failed",
      unmappedErrors:
        "The request could not be completed. Your access may have changed; refresh the page and try again.",
    };
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default onboardAccount;
