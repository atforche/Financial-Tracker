import type {
  AccountingPeriodFundViewParams,
  AccountingPeriodFundViewSearchParams,
} from "@/accounting-periods/funds/AccountingPeriodFundView";
import type {
  AccountingPeriodViewParams,
  AccountingPeriodViewSearchParams,
} from "@/accounting-periods/AccountingPeriodView";
import type { AccountingPeriodAccountViewParams } from "@/accounting-periods/accounts/AccountingPeriodAccountView";
import type { AccountingPeriodsViewSearchParams } from "@/accounting-periods/AccountingPeriodsView";
import type { CloseAccountingPeriodViewParams } from "@/accounting-periods/CloseAccountingPeriodView";
import type { CreateGoalViewParams } from "@/goals/CreateGoalView";
import type { DeleteAccountingPeriodViewParams } from "@/accounting-periods/DeleteAccountingPeriodView";
import type { DeleteGoalViewParams } from "@/goals/DeleteGoalView";
import type { ReopenAccountingPeriodViewParams } from "@/accounting-periods/ReopenAccountingPeriodView";
import type { Route } from "next";
import type { UpdateGoalViewParams } from "@/goals/UpdateGoalView";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to accounting periods.
 */
const routes = {
  index: (searchParams: AccountingPeriodsViewSearchParams): Route =>
    `/accounting-periods?${objectToSearchParams(searchParams).toString()}` as Route,
  create: "/accounting-periods/create" as Route,
  detail: (
    params: AccountingPeriodViewParams,
    searchParams: AccountingPeriodViewSearchParams,
  ): Route =>
    `/accounting-periods/${params.id}?${objectToSearchParams(searchParams).toString()}` as Route,
  close: (params: CloseAccountingPeriodViewParams): Route =>
    `/accounting-periods/${params.id}/close`,
  reopen: (params: ReopenAccountingPeriodViewParams): Route =>
    `/accounting-periods/${params.id}/reopen`,
  delete: (params: DeleteAccountingPeriodViewParams): Route =>
    `/accounting-periods/${params.id}/delete`,
  accountDetail: (params: AccountingPeriodAccountViewParams): Route =>
    `/accounting-periods/${params.id}/accounts/${params.accountId}`,
  fundDetail: (
    params: AccountingPeriodFundViewParams,
    searchParams: AccountingPeriodFundViewSearchParams,
  ): Route =>
    `/accounting-periods/${params.id}/funds/${params.fundId}?${objectToSearchParams(searchParams).toString()}`,
  goalCreate: (params: CreateGoalViewParams): Route =>
    `/accounting-periods/${params.id}/funds/${params.fundId}/goal/create`,
  goalUpdate: (params: UpdateGoalViewParams): Route =>
    `/accounting-periods/${params.id}/funds/${params.fundId}/goal/update`,
  goalDelete: (params: DeleteGoalViewParams): Route =>
    `/accounting-periods/${params.id}/funds/${params.fundId}/goal/delete`,
};

export default routes;
