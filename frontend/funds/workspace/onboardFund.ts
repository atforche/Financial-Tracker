"use server";

import type {
  FundActionPayload,
  FundActionState,
} from "@/funds/workspace/fundAction";
import type { OnboardFundRequest } from "@/funds/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of onboarding a fund.
 */
interface ActionState extends FundActionState {
  readonly nameErrors?: string | null;
  readonly descriptionErrors?: string | null;
  readonly onboardedBalanceErrors?: string | null;
  readonly regularContributionErrors?: string | null;
  readonly minimumFundedBalanceErrors?: string | null;
  readonly maximumFundedBalanceErrors?: string | null;
  readonly targetEndingBalanceErrors?: string | null;
}

/**
 * Payload for the onboarding server action.
 */
interface ActionPayload extends FundActionPayload {
  readonly request: OnboardFundRequest;
}

/**
 * Server action that onboards a new fund before any accounting periods exist.
 */
const onboardFund = async function (
  _: ActionState,
  { redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const apiClient = createApiClient();
  const { error } = await apiClient.POST("/funds/onboard", { body: request });
  if (error) {
    if (isApiError(error)) {
      const mappedError = mapApiValidationError(error, {
        [propertyName<OnboardFundRequest>("name")]: "nameErrors",
        [propertyName<OnboardFundRequest>("description")]: "descriptionErrors",
        [propertyName<OnboardFundRequest>("onboardedBalance")]:
          "onboardedBalanceErrors",
        [propertyName<OnboardFundRequest>("regularContribution")]:
          "regularContributionErrors",
        [propertyName<OnboardFundRequest>("minimumFundedBalance")]:
          "minimumFundedBalanceErrors",
        [propertyName<OnboardFundRequest>("maximumFundedBalance")]:
          "maximumFundedBalanceErrors",
        [propertyName<OnboardFundRequest>("targetEndingBalance")]:
          "targetEndingBalanceErrors",
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

export default onboardFund;
