"use server";

import dayjs, { type Dayjs } from "dayjs";
import type { CreateFundRequest } from "@/data/fundTypes";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating a fund within an accounting period.
 */
interface CreateAccountingPeriodFundState {
  readonly accountingPeriodId: string;
  readonly overallErrorMessage?: string | null;
  readonly name?: string | null;
  readonly nameErrorMessage?: string | null;
  readonly description?: string | null;
  readonly descriptionErrorMessage?: string | null;
  readonly date?: Dayjs | null;
  readonly dateErrorMessage?: string | null;
}

/**
 * Server action that creates a new fund with a given accounting period.
 */
const createAccountingPeriodFund = async function (
  { accountingPeriodId }: CreateAccountingPeriodFundState,
  formData: FormData,
): Promise<CreateAccountingPeriodFundState> {
  const name = formData.get(nameof<CreateFundRequest>("name"));
  const description = formData.get(nameof<CreateFundRequest>("description"));
  const date = formData.get(nameof<CreateFundRequest>("addDate"));

  const client = getApiClient();
  const { error } = await client.POST("/funds", {
    body: {
      name: typeof name === "string" ? name : "",
      description: typeof description === "string" ? description : "",
      accountingPeriodId,
      addDate: typeof date === "string" ? dayjs(date).format("YYYY-MM-DD") : "",
    },
  });
  if (error) {
    if (isApiError(error)) {
      let nameErrorMessage = null;
      let descriptionErrorMessage = null;
      let dateErrorMessage = null;
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() === nameof<CreateFundRequest>("name").toUpperCase()
        ) {
          nameErrorMessage = error.errors?.[key]?.[0] ?? null;
        }
        if (
          key.toUpperCase() ===
          nameof<CreateFundRequest>("description").toUpperCase()
        ) {
          descriptionErrorMessage = error.errors?.[key]?.[0] ?? null;
        }
        if (
          key.toUpperCase() ===
          nameof<CreateFundRequest>("addDate").toUpperCase()
        ) {
          dateErrorMessage = error.errors?.[key]?.[0] ?? null;
        }
      }
      return {
        accountingPeriodId,
        overallErrorMessage: error.title ?? null,
        name: typeof name === "string" ? name : "",
        nameErrorMessage,
        description: typeof description === "string" ? description : "",
        descriptionErrorMessage,
        date: typeof date === "string" ? dayjs(date) : null,
        dateErrorMessage,
      };
    }
    return {
      overallErrorMessage: "An unexpected error occurred.",
      name: typeof name === "string" ? name : "",
      description: typeof description === "string" ? description : "",
      accountingPeriodId,
    };
  }

  revalidatePath(`/accounting-periods/${accountingPeriodId}`);
  redirect(`/accounting-periods/${accountingPeriodId}`);
};

export default createAccountingPeriodFund;
