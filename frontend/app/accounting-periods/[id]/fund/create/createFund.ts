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
  readonly errorTitle?: string | null;
  readonly nameErrors?: string | null;
  readonly descriptionErrors?: string | null;
  readonly dateErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that creates a new fund.
 */
const createFund = async function (
  _: ActionState,
  request: CreateFundRequest,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST("/funds", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      let nameErrorMessage = null;
      let descriptionErrorMessage = null;
      let dateErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() === nameof<CreateFundRequest>("name").toUpperCase()
        ) {
          nameErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateFundRequest>("description").toUpperCase()
        ) {
          descriptionErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateFundRequest>("addDate").toUpperCase()
        ) {
          dateErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }
      return {
        errorTitle: error.title ?? null,
        nameErrors: nameErrorMessage,
        descriptionErrors: descriptionErrorMessage,
        dateErrors: dateErrorMessage,
        unmappedErrors: formatErrors(
          unmappedErrors.filter((e): e is string => e !== null),
        ),
      };
    }
  }

  revalidatePath(`/accounting-periods/${request.accountingPeriodId}`);
  redirect(`/accounting-periods/${request.accountingPeriodId}`);
};

export default createFund;
