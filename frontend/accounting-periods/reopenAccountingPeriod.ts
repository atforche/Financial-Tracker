"use server";

import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";
import routes from "@/framework/routes";

/**
 * Interface representing the state of reopening an accounting period.
 */
interface ActionState {
  readonly accountingPeriodId: string;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that reopens an existing accounting period.
 */
const reopenAccountingPeriod = async function ({
  accountingPeriodId,
}: ActionState): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST(
    "/accounting-periods/{accountingPeriodId}/reopen",
    {
      params: {
        path: {
          accountingPeriodId,
        },
      },
    },
  );
  if (error) {
    if (isApiError(error)) {
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
      }
      return {
        accountingPeriodId,
        errorTitle: error.title ?? null,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  const redirectUrl = routes.accountingPeriods.detail(accountingPeriodId);
  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default reopenAccountingPeriod;
