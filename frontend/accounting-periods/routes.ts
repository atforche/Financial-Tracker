import type {
  AccountingPeriodDetailViewParams,
  AccountingPeriodDetailViewSearchParams,
} from "@/accounting-periods/detail/AccountingPeriodDetailView";
import type {
  AccountingPeriodFundViewParams,
  AccountingPeriodFundViewSearchParams,
} from "@/accounting-periods/funds/AccountingPeriodFundView";
import type { AccountingPeriodAccountViewParams } from "@/accounting-periods/accounts/AccountingPeriodAccountView";
import type { AccountingPeriodIndexViewSearchParams } from "@/accounting-periods/index/AccountingPeriodIndexView";
import type { CloseAccountingPeriodViewParams } from "@/accounting-periods/close/CloseAccountingPeriodView";
import type { DeleteAccountingPeriodViewParams } from "@/accounting-periods/delete/DeleteAccountingPeriodView";
import type { ReopenAccountingPeriodViewParams } from "@/accounting-periods/reopen/ReopenAccountingPeriodView";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

/**
 * App routes related to accounting periods.
 */
const routes = {
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
};

export default routes;
