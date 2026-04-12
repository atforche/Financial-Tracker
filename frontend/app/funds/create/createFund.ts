"use server";

import type { CreateFundRequest } from "@/data/fundTypes";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating a fund.
 */
interface ActionState {
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly nameErrors?: string | null;
  readonly typeErrors?: string | null;
  readonly descriptionErrors?: string | null;
  readonly goalAmountErrors?: string | null;
  readonly accountingPeriodErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that creates a new fund.
 */
const createFund = async function (
  { redirectUrl }: ActionState,
  request: CreateFundRequest,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST("/funds", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      let nameErrorMessage = null;
      let typeErrorMessage = null;
      let descriptionErrorMessage = null;
      let goalAmountErrorMessage = null;
      let accountingPeriodErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() === nameof<CreateFundRequest>("name").toUpperCase()
        ) {
          nameErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() === nameof<CreateFundRequest>("type").toUpperCase()
        ) {
          typeErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateFundRequest>("description").toUpperCase()
        ) {
          descriptionErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateFundRequest>("goalAmount").toUpperCase()
        ) {
          goalAmountErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateFundRequest>("accountingPeriodId").toUpperCase()
        ) {
          accountingPeriodErrorMessage = formatErrors(
            error.errors?.[key] ?? null,
          );
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }
      return {
        redirectUrl,
        errorTitle: error.title ?? null,
        nameErrors: nameErrorMessage,
        typeErrors: typeErrorMessage,
        descriptionErrors: descriptionErrorMessage,
        goalAmountErrors: goalAmountErrorMessage,
        accountingPeriodErrors: accountingPeriodErrorMessage,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default createFund;
