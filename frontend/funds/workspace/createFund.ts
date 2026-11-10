"use server";

import type {
  WorkspaceActionPayload,
  WorkspaceActionState,
} from "@/framework/forms/workspaceAction";
import type { CreateFundRequest } from "@/funds/types";
import createApiClient from "@/framework/data/createApiClient";
import mapActionError from "@/framework/forms/mapActionError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating a fund.
 */
interface ActionState extends WorkspaceActionState {
  readonly nameErrors?: string | null;
  readonly descriptionErrors?: string | null;
  readonly accountingPeriodErrors?: string | null;
  readonly plannedMonthlyContributionErrors?: string | null;
  readonly minimumEndingBalanceErrors?: string | null;
  readonly maximumEndingBalanceErrors?: string | null;
}

/**
 * Payload for the create fund server action.
 */
interface ActionPayload extends WorkspaceActionPayload {
  readonly request: CreateFundRequest;
}

/**
 * Server action that creates a new fund.
 */
const createFund = async function (
  _: ActionState,
  { redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const client = await createApiClient();
  const { error } = await client.POST("/funds", {
    body: request,
  });
  if (error) {
    return mapActionError(error, {
      [propertyName<CreateFundRequest>("name")]: "nameErrors",
      [propertyName<CreateFundRequest>("description")]: "descriptionErrors",
      [propertyName<CreateFundRequest>("accountingPeriodId")]:
        "accountingPeriodErrors",
      [propertyName<CreateFundRequest>("plannedMonthlyContribution")]:
        "plannedMonthlyContributionErrors",
      [propertyName<CreateFundRequest>("minimumEndingBalance")]:
        "minimumEndingBalanceErrors",
      [propertyName<CreateFundRequest>("maximumEndingBalance")]:
        "maximumEndingBalanceErrors",
    });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default createFund;
