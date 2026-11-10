"use server";

import type {
  AccountingPeriodActionPayload,
  AccountingPeriodActionState,
} from "@/accounting-periods/workspace/accountingPeriodAction";
import createApiClient from "@/framework/data/createApiClient";
import mapActionError from "@/framework/forms/mapActionError";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Sends a request that changes an accounting period, then refreshes the page it was made from.
 */
const changeAccountingPeriod = async function (
  change: "close" | "reopen" | "delete",
  { accountingPeriodId, redirectUrl }: AccountingPeriodActionPayload,
): Promise<AccountingPeriodActionState> {
  const client = await createApiClient();
  const params = { params: { path: { accountingPeriodId } } };
  const { error } =
    change === "delete"
      ? await client.DELETE("/accounting-periods/{accountingPeriodId}", params)
      : await client.POST(
          `/accounting-periods/{accountingPeriodId}/${change}`,
          params,
        );
  if (error) {
    return mapActionError(error, {});
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

/**
 * Server action that closes an existing accounting period.
 */
const closeAccountingPeriod = async function (
  _: AccountingPeriodActionState,
  payload: AccountingPeriodActionPayload,
): Promise<AccountingPeriodActionState> {
  return changeAccountingPeriod("close", payload);
};

/**
 * Server action that reopens an existing accounting period.
 */
const reopenAccountingPeriod = async function (
  _: AccountingPeriodActionState,
  payload: AccountingPeriodActionPayload,
): Promise<AccountingPeriodActionState> {
  return changeAccountingPeriod("reopen", payload);
};

/**
 * Server action that deletes an existing accounting period and returns to the given page.
 */
const deleteAccountingPeriod = async function (
  _: AccountingPeriodActionState,
  payload: AccountingPeriodActionPayload,
): Promise<AccountingPeriodActionState> {
  const state = await changeAccountingPeriod("delete", payload);
  if (state.success === true) {
    redirect(payload.redirectUrl);
  }
  return state;
};

export {
  closeAccountingPeriod,
  deleteAccountingPeriod,
  reopenAccountingPeriod,
};
