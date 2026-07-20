"use server";

import type {
  FundActionPayload,
  FundActionState,
} from "@/funds/workspace/fundAction";
import type { CreateFundRequest } from "@/funds/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating a fund.
 */
interface ActionState extends FundActionState {
  readonly nameErrors?: string | null;
  readonly descriptionErrors?: string | null;
  readonly accountingPeriodErrors?: string | null;
  readonly regularContributionErrors?: string | null;
  readonly minimumFundedBalanceErrors?: string | null;
  readonly maximumFundedBalanceErrors?: string | null;
  readonly targetEndingBalanceErrors?: string | null;
}

/**
 * Payload for the create fund server action.
 */
interface ActionPayload extends FundActionPayload {
  readonly request: CreateFundRequest;
}

/**
 * Server action that creates a new fund.
 */
const createFund = async function (
  _: ActionState,
  { redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const client = createApiClient();
  const { error } = await client.POST("/funds", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      const mappedError = mapApiValidationError(error, {
        [propertyName<CreateFundRequest>("name")]: "nameErrors",
        [propertyName<CreateFundRequest>("description")]: "descriptionErrors",
        [propertyName<CreateFundRequest>("accountingPeriodId")]:
          "accountingPeriodErrors",
        [propertyName<CreateFundRequest>("regularContribution")]:
          "regularContributionErrors",
        [propertyName<CreateFundRequest>("minimumFundedBalance")]:
          "minimumFundedBalanceErrors",
        [propertyName<CreateFundRequest>("maximumFundedBalance")]:
          "maximumFundedBalanceErrors",
        [propertyName<CreateFundRequest>("targetEndingBalance")]:
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

export default createFund;
