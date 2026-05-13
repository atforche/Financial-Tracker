"use server";

import type { OnboardFundRequest } from "@/funds/types";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";
import routes from "@/funds/routes";

/**
 * Interface representing the state of onboarding a fund.
 */
interface ActionState {
  readonly errorTitle?: string | null;
  readonly nameErrors?: string | null;
  readonly descriptionErrors?: string | null;
  readonly onboardedBalanceErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that onboards a new fund before any accounting periods exist.
 */
const onboardFund = async function (
  _: ActionState,
  request: OnboardFundRequest,
): Promise<ActionState> {
  const apiClient = getApiClient();
  const { error } = await apiClient.POST("/funds/onboard", { body: request });
  if (error) {
    if (isApiError(error)) {
      let nameErrorMessage = null;
      let descriptionErrorMessage = null;
      let onboardedBalanceErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];

      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() === nameof<OnboardFundRequest>("name").toUpperCase()
        ) {
          nameErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<OnboardFundRequest>("description").toUpperCase()
        ) {
          descriptionErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<OnboardFundRequest>("onboardedBalance").toUpperCase()
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
        descriptionErrors: descriptionErrorMessage,
        onboardedBalanceErrors: onboardedBalanceErrorMessage,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(routes.index({}));
  redirect(routes.index({}));
};

export default onboardFund;
