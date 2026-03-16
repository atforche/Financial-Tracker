"use server";

import dayjs, { type Dayjs } from "dayjs";
import type { CreateFundRequest } from "@/data/fundTypes";
import type StateElement from "@/framework/forms/StateElement";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";
import { z } from "zod";

/**
 * Interface representing the state of creating a fund in a particular accounting period.
 */
interface ActionState {
  readonly accountingPeriodId: string;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
  readonly name?: StateElement<string | null>;
  readonly description?: StateElement<string | null>;
  readonly date?: StateElement<Dayjs | null>;
}

/**
 * Schema for validating the form data when creating a new fund in a particular accounting period.
 */
const FormSchema = z.object({
  name: z.string().min(1, "Name is required."),
  description: z.string().optional(),
  addDate: z.string().refine((value) => {
    const date = dayjs(value, "YYYY-MM-DD", true);
    return date.isValid();
  }, "A valid date is required."),
});

/**
 * Server action that creates a new fund in a particular accounting period.
 */
const createAccountingPeriodFund = async function (
  { accountingPeriodId }: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const { name, description, addDate } = FormSchema.parse({
    name: formData.get("name"),
    description: formData.get("description"),
    addDate: formData.get("addDate"),
  });

  const client = getApiClient();
  const { error } = await client.POST("/funds", {
    body: {
      name,
      description: description ?? "",
      accountingPeriodId,
      addDate,
    },
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
        accountingPeriodId,
        errorTitle: error.title ?? null,
        unmappedErrors: formatErrors(
          unmappedErrors.filter((e): e is string => e !== null),
        ),
        name: {
          value: name,
          errorMessage: nameErrorMessage,
        },
        description: {
          value: description ?? null,
          errorMessage: descriptionErrorMessage,
        },
        date: {
          value: dayjs(addDate),
          errorMessage: dateErrorMessage,
        },
      };
    }
    return {
      accountingPeriodId,
      errorTitle: "An unexpected error occurred.",
      name: {
        value: name,
      },
      description: {
        value: description ?? null,
      },
      date: {
        value: dayjs(addDate),
      },
    };
  }

  revalidatePath(`/accounting-periods/${accountingPeriodId}`);
  redirect(`/accounting-periods/${accountingPeriodId}`);
};

export default createAccountingPeriodFund;
