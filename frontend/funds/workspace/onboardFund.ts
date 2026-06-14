"use server";

import type { OnboardFundRequest } from "@/funds/types";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of onboarding a fund.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly nameErrors?: string | null;
  readonly descriptionErrors?: string | null;
  readonly onboardedBalanceErrors?: string | null;
  readonly assignmentGoalTypeErrors?: string | null;
  readonly assignmentGoalAmountErrors?: string | null;
  readonly spendingGoalTypeErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the onboarding server action.
 */
interface ActionPayload {
  readonly redirectUrl: string;
  readonly request: OnboardFundRequest;
}

/**
 * Server action that onboards a new fund before any accounting periods exist.
 */
const onboardFund = async function (
  _: ActionState,
  { redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const apiClient = getApiClient();
  const { error } = await apiClient.POST("/funds/onboard", { body: request });
  if (error) {
    if (isApiError(error)) {
      let nameErrorMessage = null;
      let descriptionErrorMessage = null;
      let onboardedBalanceErrorMessage = null;
      let assignmentGoalTypeErrorMessage = null;
      let assignmentGoalAmountErrorMessage = null;
      let spendingGoalTypeErrorMessage = null;
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
        } else if (
          key.toUpperCase() ===
          nameof<OnboardFundRequest>("assignmentGoalType").toUpperCase()
        ) {
          assignmentGoalTypeErrorMessage = formatErrors(
            error.errors?.[key] ?? null,
          );
        } else if (
          key.toUpperCase() ===
          nameof<OnboardFundRequest>("assignmentGoalAmount").toUpperCase()
        ) {
          assignmentGoalAmountErrorMessage = formatErrors(
            error.errors?.[key] ?? null,
          );
        } else if (
          key.toUpperCase() ===
          nameof<OnboardFundRequest>("spendingGoalType").toUpperCase()
        ) {
          spendingGoalTypeErrorMessage = formatErrors(
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
        assignmentGoalTypeErrors: assignmentGoalTypeErrorMessage,
        assignmentGoalAmountErrors: assignmentGoalAmountErrorMessage,
        spendingGoalTypeErrors: spendingGoalTypeErrorMessage,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  return { success: true };
};

export default onboardFund;
